using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiFinbeCore.Model;

namespace WebApiFinbeCore.Domain
{
    public class InventoryService
    {
        public static List<EstatusCredito> ObtenerEstatusDeCredito(string numeroDeCliente, string tipoLinea, string fechaInicio, string fechaFinal, int tipoRango)
        {
            List<EstatusCredito> lista = new List<EstatusCredito>();
            var service = CRMService.createService();
            string filtro = "<filter>" +
                                        "<condition attribute=\"fib_tipo\" operator=\"eq\" value=\"{0}\" />" +
                                    "</filter>";
            string condiciones = "";
            if (tipoRango == 1)
            {
                condiciones = "<condition attribute=\"fib_fechadeinicio\" operator=\"on-or-after\" value=\"{0}\" />" +
                    "<condition attribute=\"fib_fechadeinicio\" operator=\"on-or-before\" value=\"{1}\" />";
                condiciones = string.Format(condiciones, fechaInicio, fechaFinal);
            }

            if (tipoLinea.Trim().Any())
            {
                filtro = string.Format(filtro, tipoLinea);
            }
            else
            {
                filtro = "";
            }
            String fetchXmlPf = "<fetch mapping=\"logical\" version=\"1.0\">" +
                                "<entity name=\"fib_configclientesdisplinea\">" +
                                    "<attribute name=\"fib_cliente\" />" +
                                    "<attribute name=\"fib_tipo\" />" +
                                    "{1}" +
                                    "<link-entity name=\"contact\" from=\"contactid\" to=\"fib_clientepfid\" link-type=\"inner\" alias=\"contact\">" +
                                        "<attribute name=\"fib_numpersonafisica\" />" +
                                        "<attribute name=\"firstname\" />" +
                                        "<attribute name=\"lastname\" />" +
                                        "<filter>" +
                                            "<condition attribute=\"fib_numpersonafisica\" operator=\"eq\" value=\"{0}\" />" +
                                            "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                        "</filter>" +
                                        "<link-entity name=\"fib_lineadecredito\" from=\"fib_customerpfid\" to=\"contactid\" link-type=\"inner\" alias=\"linea\">" +
                                        "<attribute name=\"fib_name\" />" +
                                        "<filter>" +
                                            "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                        "</filter>" +
                                            "<link-entity name=\"fib_credito\" from=\"fib_lineadecreditoid\" to=\"fib_lineadecreditoid\" link-type=\"inner\" alias=\"credito\">" +
                                            "<attribute name=\"fib_creditoax\" />" +
                                            "<attribute name=\"fib_arrendamiento\" />" +
                                            "<attribute name=\"fib_tasa\" />" +
                                            "<attribute name=\"fib_tasaanual2\" />" +
                                            "<attribute name=\"fib_operacionaritmetica\" />" +
                                            "<attribute name=\"fib_margen\" />" +
                                            "<attribute name=\"fib_fechadeinicio\" />" +
                                            "<attribute name=\"fib_tasaarrendamiento\" />" +
                                            "<attribute name=\"fib_monto\" />" +
                                            "<attribute name=\"fib_numerodeperiodos\" />" +
                                            "<filter>" +
                                                "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                                condiciones +
                                            "</filter>" +
                                            "<link-entity name=\"fib_catalogodemoneda\" from=\"fib_catalogodemonedaid\" to=\"fib_monedaid\" link-type=\"outer\" alias=\"moneda\">" +
                                                            "<attribute name=\"fib_codigodemoneda\" />" +
                                                            "<filter>" +
                                                                "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                                            "</filter>" +
                                                    "</link-entity>" +
                                            "<link-entity name=\"fib_tasa\" from=\"fib_tasaid\" to=\"fib_tasaid\" link-type=\"outer\" alias=\"tasa\">" +
                                                            "<attribute name=\"fib_name\" />" +
                                                            "<filter>" +
                                                                "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                                            "</filter>" +
                                                    "</link-entity>" +
                                                "<link-entity name=\"fib_garantiadelineadecredito\" from=\"fib_disposicionid\" to=\"fib_creditoid\" link-type=\"inner\" alias=\"garantia\">" +

                                                "<attribute name=\"fib_modelo\" />" +
                                                "<attribute name=\"fib_noserie\" />" +
                                                "<filter>" +
                                                    "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                                "</filter>" +
                                                "<link-entity name=\"fib_tipoautomvil\" from=\"fib_tipoautomvilid\" to=\"fib_tipoautomovilid\" link-type=\"outer\" alias=\"tipoautomovil\">" +
                                                        "<attribute name=\"fib_name\" />" +
                                                        "<filter>" +
                                                            "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                                        "</filter>" +
                                                        "</link-entity>" +

                                                     "<link-entity name=\"fib_marcaauto\" from=\"fib_marcaautoid\" to=\"fib_marcaid\" link-type=\"outer\" alias=\"marca\">" +
                                                            "<attribute name=\"fib_name\" />" +
                                                            "<filter>" +
                                                                "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                                            "</filter>" +
                                                    "</link-entity>" +
                                                "</link-entity>" +
                                            "</link-entity>" +
                                        "</link-entity>" +
                                "</link-entity>" +
                                "</entity>" +
                            "</fetch>";
            String fetchXmlPm = "<fetch mapping=\"logical\" version=\"1.0\">" +
                                "<entity name=\"fib_configclientesdisplinea\">" +
                                    "<attribute name=\"fib_cliente\" />" +
                                    "<attribute name=\"fib_tipo\" />" +
                                    "{1}" +
                                    "<link-entity name=\"account\" from=\"accountid\" to=\"fib_clientepmid\" link-type=\"inner\" alias=\"account\">" +
                                        "<attribute name=\"accountnumber\" />" +
                                        "<attribute name=\"name\" />" +
                                        "<filter>" +
                                            "<condition attribute=\"accountnumber\" operator=\"eq\" value=\"{0}\" />" +
                                            "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                        "</filter>" +
                                   "    <link-entity name=\"fib_lineadecredito\" from=\"fib_customerpmid\" to=\"accountid\" link-type=\"inner\" alias=\"linea\">" +
                                            "<attribute name=\"fib_name\" />" +
                                            "<filter>" +
                                                "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                            "</filter>" +
                                            "<link-entity name=\"fib_credito\" from=\"fib_lineadecreditoid\" to=\"fib_lineadecreditoid\" link-type=\"inner\" alias=\"credito\">" +
                                                "<attribute name=\"fib_creditoax\" />" +
                                            "<attribute name=\"fib_arrendamiento\" />" +
                                            "<attribute name=\"fib_tasa\" />" +
                                            "<attribute name=\"fib_tasaanual2\" />" +
                                            "<attribute name=\"fib_operacionaritmetica\" />" +
                                            "<attribute name=\"fib_margen\" />" +
                                            "<attribute name=\"fib_fechadeinicio\" />" +
                                            "<attribute name=\"fib_tasaarrendamiento\" />" +
                                            "<attribute name=\"fib_monto\" />" +
                                            "<attribute name=\"fib_numerodeperiodos\" />" +
                                                "<filter>" +
                                                    "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                                    condiciones +
                                                "</filter>" +
                                                "<link-entity name=\"fib_catalogodemoneda\" from=\"fib_catalogodemonedaid\" to=\"fib_monedaid\" link-type=\"outer\" alias=\"moneda\">" +
                                                            "<attribute name=\"fib_codigodemoneda\" />" +
                                                            "<filter>" +
                                                                "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                                            "</filter>" +
                                                    "</link-entity>" +
                                                "<link-entity name=\"fib_tasa\" from=\"fib_tasaid\" to=\"fib_tasaid\" link-type=\"outer\" alias=\"tasa\">" +
                                                            "<attribute name=\"fib_name\" />" +
                                                            "<filter>" +
                                                                "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                                            "</filter>" +
                                                    "</link-entity>" +
                                                "<link-entity name=\"fib_garantiadelineadecredito\" from=\"fib_disposicionid\" to=\"fib_creditoid\" link-type=\"inner\" alias=\"garantia\">" +
                                                    "<attribute name=\"fib_modelo\" />" +
                                                    "<attribute name=\"fib_noserie\" />" +
                                                    "<attribute name=\"fib_descripcionunidad\" />" +

                                                    "<filter>" +
                                                        "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                                    "</filter>" +
                                                     "<link-entity name=\"fib_tipoautomvil\" from=\"fib_tipoautomvilid\" to=\"fib_tipoautomovilid\" link-type=\"outer\" alias=\"tipoautomovil\">" +
                                                        "<attribute name=\"fib_name\" />" +
                                                        "<filter>" +
                                                            "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                                        "</filter>" +
                                                        "</link-entity>" +

                                                     "<link-entity name=\"fib_marcaauto\" from=\"fib_marcaautoid\" to=\"fib_marcaid\" link-type=\"outer\" alias=\"marca\">" +
                                                            "<attribute name=\"fib_name\" />" +
                                                            "<filter>" +
                                                                "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                                            "</filter>" +
                                                    "</link-entity>" +
                                                "</link-entity>" +
                                            "</link-entity>" +
                                        "</link-entity>" +
                                    "</link-entity>" +
                                "</entity>" +
                            "</fetch>";
            string fetchXml;
            if (numeroDeCliente.Trim().StartsWith(Constantes.PERSONA_MORAL))
            {
                fetchXml = string.Format(fetchXmlPm, numeroDeCliente, filtro);
            }
            else
            {
                fetchXml = string.Format(fetchXmlPf, numeroDeCliente, filtro);
            }
            var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (results.Entities.Any())
            {
                var detallesCreditos = CustomerService.ObtenerDetalleCreditos(numeroDeCliente, "", "");
                foreach (var result in results.Entities)
                {
                    string name = "";
                    string numero = "";
                    if (numeroDeCliente.Trim().StartsWith(Constantes.PERSONA_MORAL))
                    {
                        name = string.Format("{0}", result.AtributoColleccion("account.name"));
                        numero = result.AtributoColleccion("account.accountnumber").ToStringNull();
                    }
                    else
                    {
                        name = string.Format("{0} {1}", result.AtributoColleccion("contact.firstname"), result.AtributoColleccion("contact.lastname"));
                        numero = result.AtributoColleccion("contact.fib_numpersonafisica").ToStringNull();
                    }
                    string tasaStr = "";
                    bool arrendamiento = result.AtributoColleccion("credito.fib_arrendamiento").ToBoolean();
                    if (arrendamiento)
                    {
                        tasaStr = result.AtributoColleccion("credito.fib_tasaarrendamiento").ToStringNull();
                    }
                    else
                    {
                        var valorTasa = result.AtributoColleccion("credito.fib_tasa", TipoAtributos.OPCION);
                        switch (valorTasa)
                        {
                            case Constantes.TASA_FIJA:
                                tasaStr = result.AtributoColleccion("credito.fib_tasaanual2").ToStringNull();
                                break;
                            case Constantes.TASA_VARIABLE:
                                tasaStr = string.Concat(
                                    result.AtributoColleccion("tasa.fib_name"),
                                    result.AtributoColleccion("credito.fib_operacionaritmetica", TipoAtributos.OPCION_TEXTO),
                                    result.AtributoColleccion("credito.fib_margen")
                                    );
                                break;
                        }
                    }

                    var estatusCredito = new EstatusCredito
                    {
                        noCredito = result.AtributoColleccion("credito.fib_creditoax").ToStringNull(),
                        noCliente = numero,
                        nombreCliente = name,
                        lineaCredito = result.AtributoColleccion("linea.fib_name").ToStringNull(),
                        tipoDispLinea = result.AtributoColleccion("fib_configclientesdisplinea.fib_tipo").ToStringNull(),
                        marca = result.AtributoColleccion("marca.fib_name").ToStringNull(),
                        modelo = result.AtributoColleccion("garantia.fib_modelo").ToStringNull(),
                        tipoUnidad = result.AtributoColleccion("tipoautomovil.fib_name").ToStringNull(),
                        vin = result.AtributoColleccion("garantia.fib_noserie").ToStringNull(),
                        tasa = tasaStr,
                        monto = result.AtributoColleccion("credito.fib_monto", TipoAtributos.MONEY).ToDecimal(),
                        plazo = result.AtributoColleccion("credito.fib_numerodeperiodos").ToStringNull(),
                        proximoPago = null,
                        inicioCredito = result.AtributoColleccion("credito.fib_fechadeinicio").ToStringNull(),
                        finCredito = null,
                        codigoMoneda = result.AtributoColleccion("moneda.fib_codigodemoneda").ToStringNull(),
                        descripcion = result.AtributoColleccion("garantia.fib_descripcionunidad").ToStringNull()
                    };
                    if (detallesCreditos.Any())
                    {
                        var detalleCredito = detallesCreditos.Where(dc => dc.credito.Equals(estatusCredito.noCredito)).First();
                        if (detalleCredito != null)
                        {
                            estatusCredito.finCredito = detalleCredito.FechaFinal;
                        }
                    }
                    if (tipoRango == 2)
                    {
                        string[] fechaFinCredito = estatusCredito.finCredito.Split('/');
                        string[] fechaInicioRango = fechaInicio.Split('/');
                        string[] fechaFinRango = fechaFinal.Split('/');
                        DateTime fechaInicioDate = new DateTime(int.Parse(fechaInicioRango[0]), int.Parse(fechaInicioRango[1]), int.Parse(fechaInicioRango[2]));
                        DateTime fechaFinDate = new DateTime(int.Parse(fechaFinRango[0]), int.Parse(fechaFinRango[1]), int.Parse(fechaFinRango[2]));
                        DateTime fechaFinCreditoDate = new DateTime(int.Parse(fechaFinCredito[2]), int.Parse(fechaFinCredito[1]), int.Parse(fechaFinCredito[0]));
                        if (fechaFinCreditoDate.CompareTo(fechaInicioDate) >= 0 && fechaFinCreditoDate.CompareTo(fechaFinDate) <= 0)
                        {
                            lista.Add(estatusCredito);
                        }
                    }
                    else
                    {
                        lista.Add(estatusCredito);
                    }
                }
            }
            return lista;
        }

        public static InformacionVin GetInformacionVin(string vin)
        {
            ConectorAX conectorAx = new ConectorAX();
            conectorAx.Logon();
            var ax = conectorAx.ax;
            object[] paramlist = { vin };
            string result = (string)ax.CallStaticClassMethod("AND_MicorservicesInterface", "getInfoVIN", paramlist);
            //result = "{\"conteo\":1,\"detalle\":[{\"credito\":\"CR0000007775\",\"estatus\":\"Liquidado\"}]}";
            conectorAx.Logoff();
            var informacionVin = Newtonsoft.Json.JsonConvert.DeserializeObject<InformacionVin>(result);
            return informacionVin;
        }

        public static CreditoRolesList CreditosActivos(string rfc)
        {
            CreditoRolesList rolesList = new CreditoRolesList();
            List<CreditoRol> creditos = CreditosActivosSolicitante(rfc);
            creditos.AddRange(CreditosActivosAval(rfc));
            creditos.AddRange(CreditosActivosApoderado(rfc));
            creditos.AddRange(CreditosActivosAccionista(rfc));
            rolesList.creditos = creditos;
            return rolesList;
        }

        private static List<CreditoRol> CreditosActivosSolicitante(string rfc)
        {
            var service = CRMService.createService();

            String fetchXmlPf = "<fetch mapping=\"logical\" version=\"1.0\">" +
                                "<entity name=\"contact\">" +
                                    "<attribute name=\"fib_numpersonafisica\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_rfc\" operator=\"eq\" value=\"{0}\" />" +
                                        "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                    "</filter>" +
                                    "<link-entity name=\"opportunity\" from=\"contactid\" to=\"contactid\" link-type=\"outer\" alias=\"o\">" +
                                        "<attribute name=\"fib_numcredito\" />" +
                                        "<attribute name=\"statecode\" />" +
                                            "<filter>" +
                                            "<condition attribute=\"statecode\" operator=\"in\" >" +
                                            "<value>0</value>" +
                                            "<value>1</value>" +
                                            "</condition>" +
                                        "</filter>" +
                                        "<link-entity name=\"fib_lineadecredito\" from=\"fib_solicituddecreditoid\" to=\"opportunityid\" link-type=\"outer\" alias=\"fl\">" +
                                            "<attribute name=\"fib_name\" />" +
                                                "<filter>" +
                                                "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                            "</filter>" +
                                            "<link-entity name=\"fib_credito\" from=\"fib_lineadecreditoid\" to=\"fib_lineadecreditoid\" link-type=\"outer\" alias=\"fc\">" +
                                                "<attribute name=\"fib_creditoax\" />" +
                                                "<attribute name=\"fib_calculodepagoid\" />" +
                                                "<attribute name=\"fib_impunidadsiniva\" />" +
                                                    "<filter>" +
                                                    "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                                "</filter>" +
                                            "</link-entity>" +
                                        "</link-entity>" +
                                    "</link-entity>" +
                                "</entity>" +
                            "</fetch>";
            String fetchXmlPm = "<fetch mapping=\"logical\" version=\"1.0\">" +
                                "<entity name=\"account\">" +
                                    "<attribute name=\"accountnumber\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_rfc\" operator=\"eq\" value=\"{0}\" />" +
                                        "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                    "</filter>" +
                                    "<link-entity name=\"opportunity\" from=\"accountid\" to=\"accountid\" link-type=\"outer\" alias=\"o\">" +
                                        "<attribute name=\"fib_numcredito\" />" +
                                         "<attribute name=\"statecode\" />" +
                                            "<filter>" +
                                            "<condition attribute=\"statecode\" operator=\"in\" >" +
                                            "<value>0</value>" +
                                            "<value>1</value>" +
                                            "</condition>" +
                                        "</filter>" +
                                        "<link-entity name=\"fib_lineadecredito\" from=\"fib_solicituddecreditoid\" to=\"opportunityid\" link-type=\"outer\" alias=\"fl\">" +
                                            "<attribute name=\"fib_name\" />" +
                                                "<filter>" +
                                                "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                            "</filter>" +
                                            "<link-entity name=\"fib_credito\" from=\"fib_lineadecreditoid\" to=\"fib_lineadecreditoid\" link-type=\"outer\" alias=\"fc\">" +
                                                "<attribute name=\"fib_creditoax\" />" +
                                                "<attribute name=\"fib_calculodepagoid\" />" +
                                                "<attribute name=\"fib_impunidadsiniva\" />" +
                                                    "<filter>" +
                                                    "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                                "</filter>" +
                                            "</link-entity>" +
                                        "</link-entity>" +
                                    "</link-entity>" +
                                "</entity>" +
                            "</fetch>";
            string fetchXml = "";
            if (rfc.Trim().Length == Constantes.PERSONA_MORAL_LENGTH)
            {
                fetchXml = string.Format(fetchXmlPm, rfc);
            }
            else
            {
                fetchXml = string.Format(fetchXmlPf, rfc);
            }
            List<CreditoRol> creditoRoles = new List<CreditoRol>();
            var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (results.Entities.Any())
            {
                foreach (var result in results.Entities)
                {
                    if (result.Contains("fc.fib_creditoax"))
                    {
                        var tipoDeContrato = result.AtributoColleccion("fc.fib_calculodepagoid",TipoAtributos.ENTITY_REFERENCE_NAME).ToString().ToUpper();
                        creditoRoles.Add(new CreditoRol
                        {
                            Credito = result.AtributoColleccion("fc.fib_creditoax").ToStringNull(),
                            Rol = Constantes.ROL_SOLICITANTE,
                            TipoDeContrato = tipoDeContrato.Equals("ARRENDAMIENTO") ? "Arrendamiento" : "Credito",
                            MontoInversion = tipoDeContrato.Equals("ARRENDAMIENTO") ? result.AtributoColleccion("fc.fib_impunidadsiniva",TipoAtributos.MONEY).ToDecimal2() : 0

                        });
                    }
                }
            }
            return creditoRoles;
        }


        private static List<CreditoRol> CreditosActivosAval(string rfc)
        {
            var service = CRMService.createService();

            String fetchXmlPf = "<fetch mapping=\"logical\" version=\"1.0\">" +
                                "<entity name=\"contact\">" +
                                    "<attribute name=\"fib_numpersonafisica\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_rfc\" operator=\"eq\" value=\"{0}\" />" +
                                        "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                    "</filter>" +
                                    "<link-entity name=\"fib_aval\" from=\"fib_avalpfid\" to=\"contactid\" link-type=\"outer\" alias=\"fa\">" +
                                        "<filter>" +
                                            "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                        "</filter>" +
                                        "<link-entity name=\"opportunity\" from=\"opportunityid\" to=\"fib_opportunityid\" link-type=\"outer\" alias=\"o\">" +
                                            "<attribute name=\"fib_numcredito\" />" +
                                                "<filter>" +
                                                "<condition attribute=\"statecode\" operator=\"in\" >" +
                                                "<value>0</value>" +
                                                "<value>1</value>" +
                                                "</condition>" +
                                            "</filter>" +
                                            "<link-entity name=\"fib_lineadecredito\" from=\"fib_solicituddecreditoid\" to=\"opportunityid\" link-type=\"outer\" alias=\"fl\">" +
                                                "<attribute name=\"fib_name\" />" +
                                                    "<filter>" +
                                                    "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                                "</filter>" +
                                                "<link-entity name=\"fib_credito\" from=\"fib_lineadecreditoid\" to=\"fib_lineadecreditoid\" link-type=\"outer\" alias=\"fc\">" +
                                                    "<attribute name=\"fib_creditoax\" />" +
                                                    "<attribute name=\"fib_calculodepagoid\" />" +
                                                    "<attribute name=\"fib_impunidadsiniva\" />" +
                                                        "<filter>" +
                                                        "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                                    "</filter>" +
                                                "</link-entity>" +
                                            "</link-entity>" +
                                        "</link-entity>" +
                                    "</link-entity>" +
                                "</entity>" +
                            "</fetch>";
            String fetchXmlPm = "<fetch mapping=\"logical\" version=\"1.0\">" +
                                "<entity name=\"account\">" +
                                    "<attribute name=\"accountnumber\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_rfc\" operator=\"eq\" value=\"{0}\" />" +
                                        "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                    "</filter>" +
                                    "<link-entity name=\"fib_aval\" from=\"fib_avalpmid\" to=\"accountid\" link-type=\"outer\" alias=\"fa\">" +
                                        "<filter>" +
                                            "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                        "</filter>" +
                                        "<link-entity name=\"opportunity\" from=\"opportunityid\" to=\"fib_opportunityid\" link-type=\"outer\" alias=\"o\">" +
                                            "<attribute name=\"fib_numcredito\" />" +
                                                "<filter>" +
                                                "<condition attribute=\"statecode\" operator=\"in\" >" +
                                                "<value>0</value>" +
                                                "<value>1</value>" +
                                                "</condition>" +
                                            "</filter>" +
                                            "<link-entity name=\"fib_lineadecredito\" from=\"fib_solicituddecreditoid\" to=\"opportunityid\" link-type=\"outer\" alias=\"fl\">" +
                                                "<attribute name=\"fib_name\" />" +
                                                    "<filter>" +
                                                    "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                                "</filter>" +
                                                "<link-entity name=\"fib_credito\" from=\"fib_lineadecreditoid\" to=\"fib_lineadecreditoid\" link-type=\"outer\" alias=\"fc\">" +
                                                    "<attribute name=\"fib_creditoax\" />" +
                                                    "<attribute name=\"fib_calculodepagoid\" />" +
                                                    "<attribute name=\"fib_impunidadsiniva\" />" +
                                                        "<filter>" +
                                                        "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                                    "</filter>" +
                                                "</link-entity>" +
                                            "</link-entity>" +
                                        "</link-entity>" +
                                    "</link-entity>" +
                                "</entity>" +
                            "</fetch>";
            string fetchXml = "";
            if (rfc.Trim().Length == Constantes.PERSONA_MORAL_LENGTH)
            {
                fetchXml = string.Format(fetchXmlPm, rfc);
            }
            else
            {
                fetchXml = string.Format(fetchXmlPf, rfc);
            }
            List<CreditoRol> creditoRoles = new List<CreditoRol>();
            var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (results.Entities.Any())
            {
                foreach (var result in results.Entities)
                {
                    if (result.Contains("fc.fib_creditoax"))
                    {
                        var tipoDeContrato = result.AtributoColleccion("fc.fib_calculodepagoid", TipoAtributos.ENTITY_REFERENCE_NAME).ToString().ToUpper();
                        creditoRoles.Add(new CreditoRol
                        {
                            Credito = result.AtributoColleccion("fc.fib_creditoax").ToStringNull(),
                            Rol = Constantes.ROL_AVAL,
                            TipoDeContrato = tipoDeContrato.Equals("ARRENDAMIENTO") ? "Arrendamiento" : "Credito",
                            MontoInversion = tipoDeContrato.Equals("ARRENDAMIENTO") ? result.AtributoColleccion("fc.fib_impunidadsiniva", TipoAtributos.MONEY).ToDecimal2() : 0

                        });
                    }
                }
            }
            return creditoRoles;
        }

        private static List<CreditoRol> CreditosActivosApoderado(string rfc)
        {
            var service = CRMService.createService();

            String fetchXmlPf = "<fetch mapping=\"logical\" version=\"1.0\">" +
                                "<entity name=\"contact\">" +
                                    "<attribute name=\"fib_numpersonafisica\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_rfc\" operator=\"eq\" value=\"{0}\" />" +
                                        "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                    "</filter>" +
                                    "<link-entity name=\"fib_apoderado\" from=\"fib_personafisicaid\" to=\"contactid\" link-type=\"outer\" alias=\"fa\">" +
                                        "<filter>" +
                                            "<condition attribute=\"fib_personamoralid\" operator=\"not-null\" />" +
                                            "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                        "</filter>" +
                                        "<link-entity name=\"opportunity\" from=\"accountid\" to=\"fib_personamoralid\" link-type=\"outer\" alias=\"o\">" +
                                            "<attribute name=\"fib_numcredito\" />" +
                                                "<filter>" +
                                                "<condition attribute=\"statecode\" operator=\"in\" >" +
                                                "<value>0</value>" +
                                                "<value>1</value>" +
                                                "</condition>" +
                                            "</filter>" +
                                            "<link-entity name=\"fib_lineadecredito\" from=\"fib_solicituddecreditoid\" to=\"opportunityid\" link-type=\"outer\" alias=\"fl\">" +
                                                "<attribute name=\"fib_name\" />" +
                                                    "<filter>" +
                                                    "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                                "</filter>" +
                                                "<link-entity name=\"fib_credito\" from=\"fib_lineadecreditoid\" to=\"fib_lineadecreditoid\" link-type=\"outer\" alias=\"fc\">" +
                                                    "<attribute name=\"fib_creditoax\" />" +
                                                    "<attribute name=\"fib_calculodepagoid\" />" +
                                                    "<attribute name=\"fib_impunidadsiniva\" />" +
                                                        "<filter>" +
                                                        "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                                    "</filter>" +
                                                "</link-entity>" +
                                            "</link-entity>" +
                                        "</link-entity>" +
                                    "</link-entity>" +
                                "</entity>" +
                            "</fetch>";
            
            string fetchXml = "";
            List<CreditoRol> creditoRoles = new List<CreditoRol>();
            if (!rfc.Trim().StartsWith(Constantes.PERSONA_MORAL))
            {
                fetchXml = string.Format(fetchXmlPf, rfc);

                
                var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (results.Entities.Any())
                {
                    foreach (var result in results.Entities)
                    {
                        if (result.Contains("fc.fib_creditoax"))
                        {
                            var tipoDeContrato = result.AtributoColleccion("fc.fib_calculodepagoid", TipoAtributos.ENTITY_REFERENCE_NAME).ToString().ToUpper();
                            creditoRoles.Add(new CreditoRol
                            {
                                Credito = result.AtributoColleccion("fc.fib_creditoax").ToStringNull(),
                                Rol = Constantes.ROL_APODERADO,
                                TipoDeContrato = tipoDeContrato.Equals("ARRENDAMIENTO") ? "Arrendamiento" : "Credito",
                                MontoInversion = tipoDeContrato.Equals("ARRENDAMIENTO") ? result.AtributoColleccion("fc.fib_impunidadsiniva", TipoAtributos.MONEY).ToDecimal2() : 0

                            });
                        }
                    }
                }
            }
            return creditoRoles;
        }

        private static List<CreditoRol> CreditosActivosAccionista(string rfc)
        {
            var service = CRMService.createService();

            String fetchXmlPf = "<fetch mapping=\"logical\" version=\"1.0\">" +
                                "<entity name=\"contact\">" +
                                    "<attribute name=\"fib_numpersonafisica\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_rfc\" operator=\"eq\" value=\"{0}\" />" +
                                        "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                    "</filter>" +
                                    "<link-entity name=\"fib_accionistas\" from=\"fib_accionistapfid\" to=\"contactid\" link-type=\"outer\" alias=\"fa\">" +
                                        "<filter>" +
                                            "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                        "</filter>" +
                                        "<link-entity name=\"opportunity\" from=\"accountid\" to=\"fib_personamoralid\" link-type=\"outer\" alias=\"o\">" +
                                            "<attribute name=\"fib_numcredito\" />" +
                                                "<filter>" +
                                                "<condition attribute=\"statecode\" operator=\"in\" >" +
                                                "<value>0</value>" +
                                                "<value>1</value>" +
                                                "</condition>" +
                                            "</filter>" +
                                            "<link-entity name=\"fib_lineadecredito\" from=\"fib_solicituddecreditoid\" to=\"opportunityid\" link-type=\"outer\" alias=\"fl\">" +
                                                "<attribute name=\"fib_name\" />" +
                                                    "<filter>" +
                                                    "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                                "</filter>" +
                                                "<link-entity name=\"fib_credito\" from=\"fib_lineadecreditoid\" to=\"fib_lineadecreditoid\" link-type=\"outer\" alias=\"fc\">" +
                                                    "<attribute name=\"fib_creditoax\" />" +
                                                    "<attribute name=\"fib_calculodepagoid\" />" +
                                                    "<attribute name=\"fib_impunidadsiniva\" />" +
                                                        "<filter>" +
                                                        "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                                    "</filter>" +
                                                "</link-entity>" +
                                            "</link-entity>" +
                                        "</link-entity>" +
                                    "</link-entity>" +
                                "</entity>" +
                            "</fetch>";
            String fetchXmlPm = "<fetch mapping=\"logical\" version=\"1.0\">" +
                                "<entity name=\"account\">" +
                                    "<attribute name=\"accountnumber\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_rfc\" operator=\"eq\" value=\"{0}\" />" +
                                        "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                    "</filter>" +
                                    "<link-entity name=\"fib_accionistas\" from=\"fib_accionistapmid\" to=\"accountid\" link-type=\"outer\" alias=\"fa\">" +
                                        "<filter>" +
                                            "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                        "</filter>" +
                                        "<link-entity name=\"opportunity\" from=\"accountid\" to=\"fib_personamoralid\" link-type=\"outer\" alias=\"o\">" +
                                            "<attribute name=\"fib_numcredito\" />" +
                                                "<filter>" +
                                                "<condition attribute=\"statecode\" operator=\"in\" >" +
                                                "<value>0</value>" +
                                                "<value>1</value>" +
                                                "</condition>" +
                                            "</filter>" +
                                            "<link-entity name=\"fib_lineadecredito\" from=\"fib_solicituddecreditoid\" to=\"opportunityid\" link-type=\"outer\" alias=\"fl\">" +
                                                "<attribute name=\"fib_name\" />" +
                                                    "<filter>" +
                                                    "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                                "</filter>" +
                                                "<link-entity name=\"fib_credito\" from=\"fib_lineadecreditoid\" to=\"fib_lineadecreditoid\" link-type=\"outer\" alias=\"fc\">" +
                                                    "<attribute name=\"fib_creditoax\" />" +
                                                    "<attribute name=\"fib_calculodepagoid\" />" +
                                                    "<attribute name=\"fib_impunidadsiniva\" />" +
                                                        "<filter>" +
                                                        "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                                    "</filter>" +
                                                "</link-entity>" +
                                            "</link-entity>" +
                                        "</link-entity>" +
                                    "</link-entity>" +
                                "</entity>" +
                            "</fetch>";
            string fetchXml = "";
            if (rfc.Trim().Length == Constantes.PERSONA_MORAL_LENGTH)
            {
                fetchXml = string.Format(fetchXmlPm, rfc);
            }
            else
            {
                fetchXml = string.Format(fetchXmlPf, rfc);
            }
            List<CreditoRol> creditoRoles = new List<CreditoRol>();
            var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (results.Entities.Any())
            {
                foreach (var result in results.Entities)
                {
                    if (result.Contains("fc.fib_creditoax"))
                    {
                        var tipoDeContrato = result.AtributoColleccion("fc.fib_calculodepagoid", TipoAtributos.ENTITY_REFERENCE_NAME).ToString().ToUpper();
                        creditoRoles.Add(new CreditoRol
                        {
                            Credito = result.AtributoColleccion("fc.fib_creditoax").ToStringNull(),
                            Rol = Constantes.ROL_ACCIONISTA,
                            TipoDeContrato = tipoDeContrato.Equals("ARRENDAMIENTO") ? "Arrendamiento" : "Credito",
                            MontoInversion = tipoDeContrato.Equals("ARRENDAMIENTO") ? result.AtributoColleccion("fc.fib_impunidadsiniva", TipoAtributos.MONEY).ToDecimal2() : 0

                        });
                    }
                }
            }
            return creditoRoles;
        }
    }
 }
