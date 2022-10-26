using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiFinbeCore.Model;

namespace WebApiFinbeCore.Domain
{
    public class ConfigurationService
    {
        public static ConfiguracionConexion ConfiguracionConexion()
        {
            ConectorAX conectorAx = new ConectorAX();
            conectorAx.Logon();
            var ax = conectorAx.ax;
            string result = (string)ax.CallStaticClassMethod("AND_MicorservicesInterface", "getInternetPaymentDBCon");
            conectorAx.Logoff();
            ConfiguracionConexion conexion = JsonConvert.DeserializeObject<ConfiguracionConexion>(result);
            return conexion;
        }

        public static SqlConnection ConectarBD()
        {
            string connectionFormat = "Server={0};Database={1};User Id={2};Password={3};";
            ConfiguracionConexion configuracion = ConfigurationService.ConfiguracionConexion();
            return new SqlConnection(string.Format(connectionFormat, configuracion.servidor, configuracion.bd, configuracion.usuario, configuracion.contraseña));
        }
    }
}
