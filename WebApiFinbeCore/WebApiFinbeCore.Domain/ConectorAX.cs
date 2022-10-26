using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dynamics.BusinessConnectorNet;

namespace WebApiFinbeCore.Domain
{
    public class ConectorAX
    {
        public Axapta ax;
        public static string EMPRESA = "fb";  // Direccionado a la empresa AX de Producción

        public void Logon()
        {
            ax = new Axapta();
            
            try
            {
                var usuario = ConfigurationManager.AppSettings["_userAX"].ToString();
                var password = ConfigurationManager.AppSettings["_passwordAX"].ToString();
                var dominio = ConfigurationManager.AppSettings["_domainAX"].ToString();
                var servidor = ConfigurationManager.AppSettings["_serverAX"].ToString();
                //FINBEDLL.Conecta log = new FINBEDLL.Conecta();
                //ax.Logon(empresa, "", "", "");
                //ax.Logon(log.conectar(4, 2), log.conectar(5, 2), log.conectar(6, 2), log.conectar(7, 2));
                //ax.Logon("fb", "", "ax_bcproxydes01@10.20.129.83:2712", "");
                var networkCredential = new NetworkCredential(usuario, password, dominio);
                ax.LogonAs(usuario, dominio, networkCredential,
                    EMPRESA, "", servidor, "");
                //ax.LogonAsGuest(networkCredential, "fb","", "Ax_bcproxydes01@SRVFINBEAXPBAS2:2712","");
                
                //ax.Logon("FB08", "", "FINBEAXDES1@srvfinbeaxdes1:2712", "");
                //ax.LogonAs("rgomez","adpeco",new System.Net.NetworkCredential("rgomez","palencano","adpeco"),"FB08", "", "FINBEAXDES1@srvfinbeaxdes1:2712", "");
                var response = ax.CallStaticClassMethod("SysFlushAOD", "doFlush");
            }
            catch (BusinessConnectorException e)
            {
                this.Logoff();
                throw new Exception("ErrorFB: Login 34 " + e.Message + " (" + e.GetType().ToString() + ")");
            }
            catch (Exception ex)
            {
                this.Logoff();
                throw new Exception("FB: Error Interno" + ex.Message);
            }
        }

        public void Logoff()
        {
            try
            {
                ax.Logoff();
            }
            catch (Exception ex)
            {
                throw new Exception("FB: Error Interno. " + ex.Message);
            }
        }
    }
}
