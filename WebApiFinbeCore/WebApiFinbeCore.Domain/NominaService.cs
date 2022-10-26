using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Deserializers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using WebApiFinbeCore.Model;

namespace WebApiFinbeCore.Domain
{
    public class NominaService
    {
        public static PolizaResponse ObtenerPoliza(string cia, int formato, int poliza, int anio, int mes)
        {
            var response = new PolizaResponse { Polizas = new List<Poliza>() };

            var referencias360 = new Referencias360();
            var codigoMoneda = ConfigurationManager.AppSettings["codigoMoneda"];
            referencias360.configuracionCompania = ObtenerConfiguracionCompania(cia).FirstOrDefault();
            if(referencias360.configuracionCompania==null)
                throw new Exception("Configuración de compañia no encontrada");

            var opePolizas =  ObtenerOpePolizas(referencias360.configuracionCompania, formato, poliza, anio, mes);

            if (!opePolizas.Any())
                throw new Exception("No existe información a procesar");

            referencias360.referenciaContables = ObtenerReferenciasContables(cia);
            referencias360.refDeptos = ObtenerRefDeptos(cia);
            referencias360.refSucs = ObtenerRefSuc(cia);
            referencias360.centroCostos = ObtenerCentroCosto(cia);

            if(!referencias360.centroCostos.Any())
                throw new Exception("Configuración de centro de costos no encontrada");

            var line = 1;
            foreach (var opePoliza in opePolizas)
            {
                var polizaResponse = new Poliza();
                
                polizaResponse.TransDate = opePoliza.fecha;
                polizaResponse.Description = opePoliza.descripcion + (mes + "/" + anio + "-" + formato + "-" + poliza);
                polizaResponse.CreditAmount = opePoliza.abono;
                polizaResponse.DebitAmount =  opePoliza.cargo;
                polizaResponse.Document = opePoliza.documento;
                polizaResponse.CurrencyCode = codigoMoneda;
                polizaResponse.LineNumber = line;
                
                try
                {
                    ObtenerDimensiones(referencias360, opePoliza, ref polizaResponse);   
                }
                catch(Exception ex)
                {
                    polizaResponse.TieneError = true;
                    polizaResponse.Error = ex.Message;
                }

                response.Polizas.Add(polizaResponse);
                line++;
            }

            response.TieneError = response.Polizas.Any(x => x.TieneError);
            
            return response;
        }

        private static void ObtenerDimensiones(Referencias360 referencias360, OpePoliza opePoliza, ref Poliza poliza)
        {
            ReferenciaContable referencia = referencias360.referenciaContables.FirstOrDefault(x => x.mayor == opePoliza.mayor.ToString() && x.cuenta == opePoliza.cuenta.ToString() && x.subcuenta == opePoliza.subCuenta.ToString());
            if (referencia == null)
            {
                referencia = referencias360.referenciaContables.FirstOrDefault(x => x.mayor == opePoliza.mayor.ToString() && x.cuenta == opePoliza.cuenta.ToString());

                if (referencia == null)
                    throw new Exception("Referencia Contable no encontrada, mayor: " + opePoliza.mayor + " cuenta: " + opePoliza.cuenta + " subcuenta: " + opePoliza.subCuenta.ToString());
            }

            poliza.AccountType = referencia.tipolinea;

            poliza.AccountDisplayValue = poliza.AccountType == "vendor" ? "FUN" + opePoliza.subCuenta.ToString("0000000") : referencia.ledgeraccount;

            var estrcutura = referencia.estructuradim.Split('-');
            foreach (var e in estrcutura)
            {
                switch (e)
                {
                    case "Centro de costos":
                        ConcatenaCentroCosto(referencias360.centroCostos, ref poliza);
                        break;
                    case "Departamento":
                        ConcatenaDepartamento(referencias360.refDeptos, opePoliza.subCuentaDepto, ref poliza);
                        break;
                    case "Sucursal":
                        ConcatenaSucursal(referencias360.refSucs, opePoliza.suc, ref poliza);
                        break;
                    case "Transacción":
                        ConcatenaTransaccion(referencias360.configuracionCompania.transaccion, ref poliza);
                        break;
                    case "IDGTO":
                        ConcatenaIDGTO(referencias360.configuracionCompania.idgto, ref poliza);
                        break;
                    case "":
                        poliza.DefaultDimensionDisplayValue += "-";
                        break;
                    default:
                        break;
                }
            }
        }

        private static void ConcatenaCentroCosto(List<CentroCosto> centroCostos, ref Poliza poliza)
        {
            var centroCosto = centroCostos.FirstOrDefault();

            if (centroCosto == null)
                throw new Exception("Referencia a centro de costos no encontrado en la compañia");

            poliza.DefaultDimensionDisplayValue += "-" + centroCosto.dimcc;
            poliza.CenterCost = centroCosto.dimcc;
        }

        private static void ConcatenaDepartamento(List<RefDepto> deptos, int subDepto, ref Poliza poliza)
        {
            var depto = deptos.FirstOrDefault(x => x.departamentoopeadm == subDepto.ToString());
            if (depto == null)
                throw new Exception("Referencia a departamento no encontrado en la compañia, depto: " + subDepto);
            poliza.DefaultDimensionDisplayValue += "-" + depto.dimensionnum;
            poliza.Department = depto.dimensionnum;
        }

        private static void ConcatenaSucursal(List<RefSuc> refSucs, int sucursal, ref Poliza poliza)
        {
            var referenciaSucursal = refSucs.FirstOrDefault(x => x.sucursalopeadm == sucursal.ToString());
            if (referenciaSucursal == null)
                throw new Exception("Referencia a sucursal no encontrada en la compañia, sucursal: " + sucursal);

            poliza.DefaultDimensionDisplayValue += "-" + referenciaSucursal.dimensionnum;
            poliza.Branch = referenciaSucursal.dimensionnum;
        }

        private static void ConcatenaTransaccion(string transaccion, ref Poliza poliza)
        {
            poliza.DefaultDimensionDisplayValue += "-" + transaccion;
            poliza.Transaction = transaccion;
        }

        private static void ConcatenaIDGTO(string idgto, ref Poliza poliza)
        {
            poliza.DefaultDimensionDisplayValue += "-" + idgto;
            poliza.IDGTO = idgto;
        }

        private static List<OpePoliza> ObtenerOpePolizas(ConfiguracionCompania configuracion, int formato, int poliza, int anio, int mes)
        {
            string connectionFormat = "Server={0};Database={1};User Id={2};Password={3};";
            var connection = new SqlConnection(string.Format(connectionFormat, configuracion.ip, configuracion.bd, configuracion.usr, configuracion.pwd));
            connection.Open();
            var query = @"select cia, suc, -- sucursal D365
                        formato, poliza, mes, ano, --datos de entrada
                        ctadepto, sctadepto, --depto D365
                        ctaarea, sctaarea, --N / A
                        mayor, cuenta, scuenta, --cuenta D365
                        fecha, descrip,
                        ctacentro, sctacentro, --CentroCosto D365
                        cargo = sum(cargo), abono = sum(abono), docto
                        from oa_polizas
                        where formato = " + formato + " and ano = " + anio + " and mes = " + mes + " and poliza = " + poliza + @"
                        group by cia, suc, formato, poliza, mes, ano, ctacentro, sctacentro, ctadepto, sctadepto, ctaarea, sctaarea, mayor, cuenta, scuenta,fecha, descrip, docto
                        having sum(cargo + abono) != 0
                        order by poliza asc";
            var command = new SqlCommand(query, connection);
            
            var opePolizas = new List<OpePoliza>();
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    opePolizas.Add(new OpePoliza
                    {
                        cia = reader.GetInt16(0),
                        suc = reader.GetByte(1),
                        formato = reader.GetInt32(2),
                        poliza = reader.GetInt32(3),
                        mes = reader.GetByte(4),
                        anio = reader.GetInt16(5),
                        cuentaDepto = reader.GetInt32(6),
                        subCuentaDepto = reader.GetInt32(7),
                        cuentaArea = reader.GetInt32(8),
                        subCuentaArea = reader.GetInt32(9),
                        mayor = reader.GetInt32(10),
                        cuenta = reader.GetInt32(11),
                        subCuenta = reader.GetInt32(12),
                        fecha = reader.GetDateTime(13),
                        descripcion = reader.GetString(14),
                        cuentaCentro = reader.GetInt32(15),
                        subCuentaCentro = reader.GetInt32(16),
                        cargo = reader.GetDecimal(17),
                        abono = reader.GetDecimal(18),
                        documento = reader.IsDBNull(19) ? string.Empty : reader.GetString(19),
                    });
                }
            }

            return opePolizas;
        }

        private static List<ConfiguracionCompania> ObtenerConfiguracionCompania(string cia)
        {
            return Api360Service<List<ConfiguracionCompania>>.Exec("/api/ConfigDeCompanias/" + cia);
        }

        private static List<ReferenciaContable> ObtenerReferenciasContables(string cia)
        {
            return Api360Service<List<ReferenciaContable>>.Exec("/api/ReferenciasCContables/" + cia);
        }

        private static List<CentroCosto> ObtenerCentroCosto(string cia)
        {
            return Api360Service<List<CentroCosto>>.Exec("/api/CentroCosto/" + cia);
        }

        private static List<RefDepto> ObtenerRefDeptos(string cia)
        {
            return Api360Service<List<RefDepto>>.Exec("/api/RefDeptos/" + cia);
        }

        private static List<RefSuc> ObtenerRefSuc(string cia)
        {
            return Api360Service<List<RefSuc>>.Exec("/api/RefSucs/" + cia);
        }
    }
}
