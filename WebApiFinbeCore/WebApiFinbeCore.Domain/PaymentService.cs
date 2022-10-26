using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiFinbeCore.Model;

namespace WebApiFinbeCore.Domain
{
    public class PaymentService
    {
        public static bool RegistroPago(Pago pago)
        {
            bool response = false;
            SqlConnection sqlConnection = null;
            SqlTransaction transaction = null;
            try
            {
                sqlConnection = ConfigurationService.ConectarBD();
                sqlConnection.Open();
                transaction = sqlConnection.BeginTransaction();
                SqlCommand command = new SqlCommand("INSERT INTO finan.pagosportal (custid,referencia,numauto,importe,estatus,fecha)" +
                    "values (@custid,@referencia,@numauto,@importe,@estatus,@fecha)");
                command.Connection = sqlConnection;
                command.Transaction = transaction;
                command.Prepare();
                command.Parameters.AddWithValue("@custid", pago.NoCliente);
                command.Parameters.AddWithValue("@referencia", pago.Referencia);
                command.Parameters.AddWithValue("@numauto", pago.NoAuth == null? "" : pago.NoAuth);
                command.Parameters.AddWithValue("@importe", pago.MontoGlobal);
                command.Parameters.AddWithValue("@estatus", pago.Estatus == null ? "" : pago.Estatus);
                command.Parameters.AddWithValue("@fecha", pago.Fecha);
                command.ExecuteNonQuery();
                if (pago.Detalle.Any())
                {
                    foreach (var detalle in pago.Detalle)
                    {
                        SqlCommand commandDetalle = new SqlCommand("INSERT INTO finan.detallepagos (referencia,creditoid,importe)" +
                        "values (@referencia,@creditoid,@importe)");
                        commandDetalle.Connection = sqlConnection;
                        commandDetalle.Transaction = transaction;
                        commandDetalle.Prepare();
                        commandDetalle.Parameters.AddWithValue("@referencia", detalle.Referencia);
                        commandDetalle.Parameters.AddWithValue("@creditoid", detalle.Credito);
                        commandDetalle.Parameters.AddWithValue("@importe", detalle.Monto);
                        commandDetalle.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
                sqlConnection.Close();
                response = true;
            }catch(Exception e)
            {
                if(sqlConnection != null)
                {
                    transaction.Rollback();
                    sqlConnection.Close();
                }
                throw e;
            }
            return response;
        }

        public static bool ActualizarPago(ActualizaPago pago)
        {
            bool response = false;
            SqlConnection sqlConnection = null;
            SqlTransaction transaction = null;
            try
            {
                sqlConnection = ConfigurationService.ConectarBD();
                sqlConnection.Open();
                transaction = sqlConnection.BeginTransaction();
                SqlCommand command = new SqlCommand("UPDATE finan.pagosportal SET custid = @custid,numauto = @numauto,estatus = @estatus WHERE referencia = @referencia");
                command.Connection = sqlConnection;
                command.Transaction = transaction;
                command.Prepare();
                command.Parameters.AddWithValue("@custid", pago.NoCliente);
                command.Parameters.AddWithValue("@referencia", pago.Referencia);
                command.Parameters.AddWithValue("@numauto", pago.NoAuth);
                command.Parameters.AddWithValue("@estatus", pago.Estatus);
                command.ExecuteNonQuery();
                transaction.Commit();
                sqlConnection.Close();
                response = true;
            }
            catch (Exception e)
            {
                if (sqlConnection != null)
                {
                    transaction.Rollback();
                    sqlConnection.Close();
                }
                throw e;
            }
            return response;
        }

        public static bool ExistePago(string referencia,string noCliente)
        {
            bool response = false;
            SqlConnection sqlConnection = null;
            SqlTransaction transaction = null;
            try
            {
                sqlConnection = ConfigurationService.ConectarBD();
                sqlConnection.Open();
                transaction = sqlConnection.BeginTransaction();
                SqlCommand command = new SqlCommand("SELECT count(*) FROM finan.pagosportal WHERE referencia = @referencia and custid = @custid");
                command.Connection = sqlConnection;
                command.Transaction = transaction;
                command.Prepare();
                command.Parameters.AddWithValue("@referencia", referencia);
                command.Parameters.AddWithValue("@custid", noCliente);
                int? count = (int?)command.ExecuteScalar();
                if(count != null && count > 0)
                {
                    response = true;
                }
                transaction.Commit();
                sqlConnection.Close();
            }
            catch(Exception e)
            {
                if (sqlConnection != null)
                {
                    transaction.Rollback();
                    sqlConnection.Close();
                }
                throw e;
            }
            return response;
        }


        public static List<PagoPendiente> ObtenerPagosPendientes(string ClienteId)
        {
            SqlConnection sqlConnection = null;
            SqlTransaction transaction = null;
            SqlDataReader reader = null;
            List<PagoPendiente> pagosPendientes = new List<PagoPendiente>();
            try
            {
                sqlConnection = ConfigurationService.ConectarBD();
                sqlConnection.Open();
                transaction = sqlConnection.BeginTransaction();
                SqlCommand command = new SqlCommand(
                    " select finan.detallepagos.referencia, finan.detallepagos.creditoid, finan.detallepagos.importe,  finan.pagosportal.fecha,  finan.pagosportal.estatus " +
                    " from finan.pagosportal inner join finan.detallepagos on (finan.pagosportal.referencia = finan.detallepagos.referencia) " +
                    " where custid = @clienteId " +
                    " order by finan.pagosportal.fecha asc, finan.detallepagos.referencia, finan.pagosportal.estatus ");
                command.Connection = sqlConnection;
                command.Transaction = transaction;
                command.Prepare();
                command.Parameters.AddWithValue("@clienteId", ClienteId);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    pagosPendientes.Add(new PagoPendiente
                    {
                        referencia  = reader["referencia"].ToString(),
                        credito = reader["creditoid"].ToString(),
                        importe = reader["importe"].ToDecimal(),
                        fecha = reader["fecha"].ToString(),
                        estatus = reader["estatus"].ToString()
                    });
                }
                reader.Close();
                transaction.Commit();
                sqlConnection.Close();
            }
            catch (Exception)
            {
                
                if(reader != null)
                {
                    reader.Close();
                }
                if(sqlConnection != null)
                {
                    transaction.Rollback();
                    sqlConnection.Close();
                }
            }
            return pagosPendientes;
        }

        
    }
}
