using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebApiFinbeCore.Model;
namespace WebApiFinbeCore.Domain
{
    public class CustomerService
    {

        public static List<ClientesPlanPiso> ObtenerClientesPlanPiso(string tipo = "")
        {
            List<ClientesPlanPiso> customers = new List<ClientesPlanPiso>();
            var service = CRMService.createService();
            String filtro = "<filter>" +
                                        "<condition attribute=\"fib_tipo\" operator=\"eq\" value=\"{0}\" />" +
                                    "</filter>";
            if (tipo.Trim().Any())
            {
                filtro = string.Format(filtro, tipo);
            }
            else
            {
                filtro = "";
            }
            String fetchXml = "<fetch mapping=\"logical\" version=\"1.0\">" +
                                "<entity name=\"fib_configclientesdisplinea\">" +
                                    "<attribute name=\"fib_cliente\" />" +
                                    "<attribute name=\"fib_tipo\" />" +
                                    filtro +
                                    "<link-entity name=\"account\" from=\"accountid\" to=\"fib_clientepmid\" link-type=\"outer\" alias=\"account\">" +
                                        "<attribute name=\"accountnumber\" />" +
                                        "<attribute name=\"name\" />" +
                                        "<attribute name=\"fib_nombrecomercial\" />" +
                                        "<attribute name=\"address1_line1\" />" +
                                        "<attribute name=\"fib_coloniaid\" />" +
                                        "<attribute name=\"address1_postalcode\" />" +
                                        "<attribute name=\"fib_municipioid\" />" +
                                        "<attribute name=\"new_estadoid\" />" +
                                        "<filter>" +
                                            "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                        "</filter>" +
                                    "</link-entity>" +
                                    "<link-entity name=\"contact\" from=\"contactid\" to=\"fib_clientepfid\" link-type=\"outer\" alias=\"contact\">" +
                                        "<attribute name=\"fib_numpersonafisica\" />" +
                                        "<attribute name=\"fullname\" />" +
                                        "<attribute name=\"address1_line1\" />" +
                                        "<attribute name=\"fib_coloniaid\" />" +
                                        "<attribute name=\"address1_postalcode\" />" +
                                        "<attribute name=\"fib_delegmposid\" />" +
                                        "<attribute name=\"new_estadoid\" />" +
                                        "<filter>" +
                                            "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                        "</filter>" +
                                    "</link-entity>" +
                                "</entity>" +
                            "</fetch>";
            var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (results.Entities.Any())
            {
                foreach (var result in results.Entities)
                {
                    if (result.Attributes.Contains("account.accountnumber"))
                    {
                        string colonia = result.AtributoColleccion("account.fib_coloniaid", TipoAtributos.ENTITY_REFERENCE_NAME).ToStringNull();
                        string municipio = result.AtributoColleccion("account.fib_municipioid", TipoAtributos.ENTITY_REFERENCE_NAME).ToStringNull();
                        string estado = result.AtributoColleccion("account.new_estadoid", TipoAtributos.ENTITY_REFERENCE_NAME).ToStringNull();
                        customers.Add(new ClientesPlanPiso
                        {
                            numeroCliente = result.AtributoColleccion("account.accountnumber").ToStringNull(),
                            razonSocial = result.AtributoColleccion("account.name").ToStringNull(),
                            nombreEmpresa = result.AtributoColleccion("account.fib_nombrecomercial").ToStringNull(),
                            direccionFiscal = result.AtributoColleccion("account.address1_line1").ToStringNull() +
                            " COLONIA " + (colonia != null ? (colonia.Contains("-") ? colonia.Split('-')[1].Trim() : colonia) : "") + ", " +
                            "CÓDIGO POSTAL " + (result.AtributoColleccion("account.address1_postalcode").ToStringNull()) + ", " +
                            (municipio != null ? (municipio.Contains("-") ? municipio.Split('-')[1].Trim() : municipio) : "") + " " +
                            (estado != null ? (estado.Contains("-") ? estado.Split('-')[1].Trim() : estado) : ""),
                            tipoDisLinea = result.AtributoColleccion("fib_tipo", TipoAtributos.OPCION).ToStringNull()
                        });
                    }
                    if (result.Attributes.Contains("contact.fib_numpersonafisica"))
                    {
                        string colonia = result.AtributoColleccion("contact.fib_coloniaid", TipoAtributos.ENTITY_REFERENCE_NAME).ToStringNull();
                        string municipio = result.AtributoColleccion("contact.fib_delegmposid", TipoAtributos.ENTITY_REFERENCE_NAME).ToStringNull();
                        string estado = result.AtributoColleccion("contact.new_estadoid", TipoAtributos.ENTITY_REFERENCE_NAME).ToStringNull();
                        customers.Add(new ClientesPlanPiso
                        {
                            numeroCliente = result.AtributoColleccion("contact.fib_numpersonafisica").ToStringNull(),
                            razonSocial = result.AtributoColleccion("contact.fullname").ToStringNull(),
                            nombreEmpresa = result.AtributoColleccion("contact.fullname").ToStringNull(),
                            direccionFiscal = result.AtributoColleccion("contact.address1_line1").ToStringNull() +
                            " COLONIA " + (colonia != null ? (colonia.Contains("-") ? colonia.Split('-')[1].Trim() : colonia) : "") + ", " +
                            "CÓDIGO POSTAL " + (result.AtributoColleccion("contact.address1_postalcode").ToStringNull()) + ", " +
                            (municipio != null ? (municipio.Contains("-") ? municipio.Split('-')[1].Trim() : municipio) : "") + " " +
                            (estado != null ? (estado.Contains("-") ? estado.Split('-')[1].Trim() : estado) : ""),
                            tipoDisLinea = result.AtributoColleccion("fib_tipo", TipoAtributos.OPCION).ToStringNull()
                        });
                    }
                }
            }
            return customers;
        }

        public static List<RepresentanteLegal> ObtenerRepresentantesLegales(string numeroCliente)
        {
            List<RepresentanteLegal> customers = new List<RepresentanteLegal>();

            var service = CRMService.createService();
            String fetchXmlPM = "<fetch mapping=\"logical\" version=\"1.0\">" +
                                "<entity name=\"account\">" +
                                    "<attribute name=\"accountnumber\" />" +
                                    "<filter>" +
                                            "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                            "<condition attribute=\"accountnumber\" operator=\"eq\" value=\"{0}\" />" +
                                    "</filter>" +
                                    "<link-entity name=\"fib_apoderado\" from=\"fib_personamoralid\" to=\"accountid\" link-type=\"inner\" alias=\"apoderado\">" +
                                        "<attribute name=\"fib_personamoralid\" />" +
                                        "<attribute name=\"fib_personafisicaid\" />" +
                                        "<attribute name=\"fib_cargoid\" />" +
                                        "<filter>" +
                                            "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                        "</filter>" +
                                        "<link-entity name=\"contact\" from=\"contactid\" to=\"fib_personafisicaid\" link-type=\"inner\" alias=\"contact\">" +
                                        "<attribute name=\"fib_numpersonafisica\" />" +
                                        "<attribute name=\"firstname\" />" +
                                        "<attribute name=\"middlename\" />" +
                                        "<attribute name=\"lastname\" />" +
                                        "<attribute name=\"fib_apellidomaterno\" />" +
                                        "<attribute name=\"fib_rfc\" />" +
                                        "<attribute name=\"emailaddress1\" />" +
                                        "<attribute name=\"telephone1\" />" +
                                        "<attribute name=\"address1_line1\" />" +
                                        "<attribute name=\"fib_coloniaid\" />" +
                                        "<attribute name=\"address1_postalcode\" />" +
                                        "<attribute name=\"fib_delegmposid\" />" +
                                        "<attribute name=\"new_estadoid\" />" +

                                        "<filter>" +
                                            "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                        "</filter>" +
                                        "</link-entity>" +
                                    "</link-entity>" +
                                "</entity>" +
                            "</fetch>";

            String fetchXmlPF = "<fetch mapping=\"logical\" version=\"1.0\">" +
                                "<entity name=\"contact\">" +
                                    "<attribute name=\"fib_numpersonafisica\" />" +
                                    "<attribute name=\"firstname\" />" +
                                    "<attribute name=\"middlename\" />" +
                                    "<attribute name=\"lastname\" />" +
                                    "<attribute name=\"fib_apellidomaterno\" />" +
                                    "<attribute name=\"fib_rfc\" />" +
                                    "<attribute name=\"emailaddress1\" />" +
                                    "<attribute name=\"telephone1\" />" +
                                    "<attribute name=\"address1_line1\" />" +
                                    "<attribute name=\"fib_coloniaid\" />" +
                                    "<attribute name=\"address1_postalcode\" />" +
                                    "<attribute name=\"fib_delegmposid\" />" +
                                    "<attribute name=\"new_estadoid\" />" +
                                    "<filter>" +
                                            "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                            "<condition attribute=\"fib_numpersonafisica\" operator=\"eq\" value=\"{0}\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";

            if (numeroCliente.Trim().StartsWith(Constantes.PERSONA_MORAL))
            {
                var results = service.RetrieveMultiple(new FetchExpression(string.Format(fetchXmlPM, numeroCliente)));
                if (results.Entities.Any())
                {
                    foreach (var result in results.Entities)
                    {
                        string colonia = result.AtributoColleccion("contact.fib_coloniaid",TipoAtributos.ENTITY_REFERENCE_NAME).ToStringNull();
                        string municipio = result.AtributoColleccion("contact.fib_delegmposid", TipoAtributos.ENTITY_REFERENCE_NAME).ToStringNull();
                        string estado = result.AtributoColleccion("contact.new_estadoid", TipoAtributos.ENTITY_REFERENCE_NAME).ToStringNull();
                        customers.Add(new RepresentanteLegal
                        {
                            numeroCliente = result.AtributoColleccion("contact.fib_numpersonafisica").ToStringNull(),
                            nombres = ((result.AtributoColleccion("contact.firstname").ToStringNull() != null ? (result.AtributoColleccion("contact.firstname").ToString() + " ") : "") +
                                    (result.AtributoColleccion("contact.middlename").ToStringNull() != null ? (result.AtributoColleccion("contact.middlename").ToString()) : "")).Trim(),
                            apellidos = ((result.AtributoColleccion("contact.lastname").ToStringNull() != null ? (result.AtributoColleccion("contact.lastname").ToString() + " ") : "") +
                                    (result.AtributoColleccion("contact.fib_apellidomaterno").ToStringNull() != null ? (result.AtributoColleccion("contact.fib_apellidomaterno").ToString()) : "")).Trim(),
                            rfc = result.AtributoColleccion("contact.fib_rfc").ToStringNull(),
                            correo = result.AtributoColleccion("contact.emailaddress1").ToStringNull(),
                            direccionFiscal = result.AtributoColleccion("contact.address1_line1").ToStringNull() != null ? (result.AtributoColleccion("contact.address1_line1").ToStringNull() +
                            " COLONIA " + (colonia!= null ? (colonia.Contains("-")?colonia.Split('-')[1].Trim() : colonia) : "") + ", " +
                            "CÓDIGO POSTAL " + (result.AtributoColleccion("contact.address1_postalcode").ToStringNull()) +", " +
                            (municipio != null ? (municipio.Contains("-") ? municipio.Split('-')[1].Trim() : municipio) : "") + " "+
                            (estado != null ? (estado.Contains("-") ? estado.Split('-')[1].Trim() : estado) : "")):null,
                            telefono = result.AtributoColleccion("contact.telephone1").ToStringNull(),
                            cargo = result.AtributoColleccion("apoderado.fib_cargoid", TipoAtributos.ENTITY_REFERENCE_NAME).ToStringNull()
                    });
                    }
                }
            }
            else
            {
                var results = service.RetrieveMultiple(new FetchExpression(string.Format(fetchXmlPF, numeroCliente)));
                if (results.Entities.Any())
                {
                    foreach (var result in results.Entities)
                    {
                        string colonia = result.AtributoColleccion("fib_coloniaid", TipoAtributos.ENTITY_REFERENCE_NAME).ToStringNull();
                        string municipio = result.AtributoColleccion("fib_delegmposid", TipoAtributos.ENTITY_REFERENCE_NAME).ToStringNull();
                        string estado = result.AtributoColleccion("new_estadoid", TipoAtributos.ENTITY_REFERENCE_NAME).ToStringNull();
                        customers.Add(new RepresentanteLegal
                        {
                            numeroCliente = result.AtributoColleccion("fib_numpersonafisica").ToStringNull(),
                            nombres = ((result.AtributoColleccion("firstname").ToStringNull() != null ? (result.AtributoColleccion("firstname").ToString() + " ") : "") +
                                    (result.AtributoColleccion("middlename").ToStringNull() != null ? (result.AtributoColleccion("middlename").ToString()) : "")).Trim(),
                            apellidos = ((result.AtributoColleccion("lastname").ToStringNull() != null ? (result.AtributoColleccion("lastname").ToString() + " ") : "") +
                                    (result.AtributoColleccion("fib_apellidomaterno").ToStringNull() != null ? (result.AtributoColleccion("fib_apellidomaterno").ToString()) : "")).Trim(),
                            rfc = result.AtributoColleccion("fib_rfc").ToStringNull(),
                            correo = result.AtributoColleccion("emailaddress1").ToStringNull(),
                            direccionFiscal = result.AtributoColleccion("address1_line1").ToStringNull() != null ? (result.AtributoColleccion("address1_line1").ToStringNull() +
                            " COLONIA " + (colonia != null ? (colonia.Contains("-") ? colonia.Split('-')[1].Trim() : colonia) : "") + ", " +
                            "CÓDIGO POSTAL " + (result.AtributoColleccion("address1_postalcode").ToStringNull()) + ", " +
                            (municipio != null ? (municipio.Contains("-") ? municipio.Split('-')[1].Trim() : municipio) : "") + " " +
                            (estado != null ? (estado.Contains("-") ? estado.Split('-')[1].Trim() : estado) : "")): null,
                            telefono = result.AtributoColleccion("telephone1").ToStringNull(),
                            cargo = null
                        });
                    }
                }
            }

            return customers;
        }

        public static string obtenerReferenciaBancaria(string idCliente)
        {
            ConectorAX conectorAx = new ConectorAX();
            conectorAx.Logon();
            var ax = conectorAx.ax;
            object[] paramlist = { idCliente };
            string result = (string)ax.CallStaticClassMethod("AND_MicorservicesInterface", "getCustomerBankReference", paramlist);
            conectorAx.Logoff();
            return result;
        }

        public static List<Configuracion> getConfiguracion(String noCliente, string tipoLinea)
        {
            List<Configuracion> configuraciones = new List<Configuracion>();

            try
            {
                bool tieneSaldosPendientes = false;
                bool puedeDisponer = false;
                string error = "";
                // Si el cliente tiene saldos exigibles, no puede realizar disposiciones en línea
                if (tieneExigible(noCliente))
                {
                    tieneSaldosPendientes = true;
                    error = "Se ha detectado que cuenta con importes exigibles, podrá realizar disposiciones hasta que liquide la cantidad pendiente.";
                }

                String clienteId = getClienteId(noCliente);
                List<Configurador> configuradores = getConfiguradores(clienteId, tipoLinea);
                
                foreach (var configurador in configuradores)
                {
                    String lineacreditoId = configurador.lineaDeCreditoId;
                    Configuracion conf = new Configuracion();
                    conf.tipoDispLinea = configurador.tipoDispLinea;
                    conf.lineaCredito = configurador.lineaCredito;
                    int diadecorte;

                    if (configurador == null || configurador.lineaDeCreditoId == null)
                    {
                        throw new Exception("FB-DL 1: El cliente " + noCliente + " no puede realizar Disposiciones en Línea");
                    }
                    else
                    {
                        conf.puedeDisponer = true;
                    }

                    if (configurador.status == null || !configurador.status.Equals("2"))
                    {
                        throw new Exception("FB-DL 1: El cliente " + noCliente + " no puede realizar Disposiciones en Línea, la Línea de Crédito no está aprobada");
                    }
                    else
                    {
                        conf.puedeDisponer = true;
                    }

                    DisponiblePorProducto disponible = getDisponible(lineacreditoId);

                    decimal montoDisponible = disponible.importeDisponible;

                    conf.montoMaximo = montoDisponible;

                    montoDisponible -= disponible.importeDispuesto;
                    conf.disponible = montoDisponible;

                    montoDisponible += disponible.importeRevolvente;
                    conf.montoRestante = montoDisponible;

                    if (configurador.diaDeCorte != null)
                    {
                        diadecorte = (int)configurador.diaDeCorte;
                        conf.diaDeCorte = diadecorte;
                    }
                    else
                    {
                        diadecorte = DateTime.Now.Day;
                    }

                    if (configurador.impuesto != null)
                    {
                        conf.ivaGeneral = configurador.impuesto;
                        if (!getImpuestoCliente(noCliente).Equals("4"))
                        {
                            conf.impuesto = configurador.impuesto;
                        }
                    }
                    else
                    {
                        conf.ivaGeneral = 0;
                        conf.impuesto = 0;
                    }

                    if (configurador.comision != null)
                    {
                        conf.comision = configurador.comision;
                    }

                    String plazos = configurador.plazosDePagoNombres;
                    plazos = Regex.Replace(plazos.ToLower(), "[^,0-9]", "");
                    String[] aPlazos = plazos.Split(',');
                    plazos = "";

                    // Validar plazos disponibles para la Línea de Crédito del Cliente
                    //DateTime fechaVencimientoLC = configurador.fechaVencimiento;
                    //DateTime fechaPrimerPago = Utilidades.CalcularFechaPerGracia(DateTime.Now, diadecorte);

                    for (int i = 0; i < aPlazos.Length; i++)
                    {
                        int plazo = int.Parse(aPlazos[i]);

                        //if (DateTime.Compare(fechaPrimerPago.AddMonths(plazo - 1), fechaVencimientoLC) <= 0)
                        //    plazos += plazo + "|";

                        if (ValidarPlazo(configurador.fechaVencimiento, diadecorte, configurador.diaCambioMes.HasValue ? configurador.diaCambioMes.Value : diadecorte, plazo - 1))
                            plazos += plazo + "|";
                    }

                    plazos = plazos.TrimEnd('|');

                    if (plazos.Length == 0)
                    {
                        conf.error = "No se pueden realizar disposiciones, los plazos exceden la fecha de vencimiento de la línea de crédito.";
                    }
                    else
                    {
                        conf.puedeDisponer = true;
                    }
                    conf.plazos = plazos;


                    conf.creditoEmpresarial = configurador.creditoEmpresarial;

                    String producto = getProducto(lineacreditoId);
                    conf.producto = producto;

                    if (configurador.bancoId != null)
                    {
                        conf.banco = configurador.bancoName;
                    }


                    if (configurador.cuenta != null)
                    {
                        conf.cuenta = configurador.cuenta;
                    }

                    if (configurador.clabe != null)
                    {
                        conf.clabe = configurador.clabe; ;
                    }

                    if(error.Trim().Length > 0)
                    {
                        conf.errorExigibles = error;
                    }
                    conf.tienesSaldosPendientes = tieneSaldosPendientes;
                    conf.tasaMora = configurador.tasaMora;
                    conf.tasaInteres = configurador.tasaInteres;
                    conf.tasaFijaVariable = configurador.tasaFijaVariable;
                    conf.valorTasaVariable = configurador.valorTasaVariable;
                    configuraciones.Add(conf);
                }

            }
            catch (ConectorCRMException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception("FB-DL 2: Error al recuperar información del cliente " + noCliente);
            }

            return configuraciones;
        }

        private static bool ValidarPlazo(DateTime fechaVencimiento, int diaDeCorte, int diaCambioMes, int plazo)
        {
            var meses = 0;
            if (DateTime.Now.Day < diaCambioMes)
                meses = 1;
            else
                meses = 2;

            var fecha = new DateTime(DateTime.Now.Year, DateTime.Now.Month + meses, diaDeCorte);
            if (fecha.AddMonths(plazo) > fechaVencimiento)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Método para la autenticación de clientes
        /// </summary>
        /// <param name="configuradorId">Guid del cliente (PF o PM) en el CRM  de FINBE</param>
        /// <param name="clave">Clave de operaciones del cliente</param>
        /// <returns>
        /// Tipo de autenticación:
        /// 0 = No Válido
        /// 1 = Autenticado con clave de un solo uso
        /// 2 = Autenticado con cambio de clave requerido
        /// 3 = Autenticado
        /// 4 = Bloqueado
        /// </returns>
        public static Login Autenticar(string numeroDeCliente, string clave)
        {
            Login login = null;
            String aviso = "";
            if (clave == null || clave.Replace(" ", String.Empty).Length == 0)
            {
                return login;
            }
            else
            {
                clave = clave.Trim();
            }
            try
            {
                var service = CRMService.createService();

                String clienteId = getClienteId(numeroDeCliente);
                Configurador config = getConfiguradores(clienteId)[0];
                var configuradorId = new Guid(config.id);

                Entity configurador = new Entity("fib_configclientesdisplinea");
                configurador.Id = configuradorId;

                configurador.Attributes.Add("fib_clavecheck", clave);
                aviso = "antes update";
                service.Update(configurador);
                aviso = "despues update";
                configurador = service.Retrieve("fib_configclientesdisplinea", configuradorId, new ColumnSet("fib_autenticacion"));
                aviso = "autenticacion";
                if (configurador.Attributes.Contains("fib_autenticacion"))
                {
                    login = new Login();
                    aviso = "entro";
                    int autenticacion = ((OptionSetValue)configurador["fib_autenticacion"]).Value - 1;
                    login.autenticacion = autenticacion;   // Ajuste del valor del picklist, los cuales inician en 1
                    aviso = "intentos";
                    int maxIntentos = MaxIntentosAutenticacion();
                    login.maxIntentos = maxIntentos;
                }
            }
            catch (Exception)
            {
                throw new ConectorCRMException("FB-CL 2: Error al autenticar al cliente" + aviso);
            }
            return login;
        }


        public static List<BitacoraDisposicion> getBitacorasDisposicion(string numeroDeCliente)
        {
            List<BitacoraDisposicion> bitacoras = new List<BitacoraDisposicion>();

            String fetchXml = "<fetch mapping=\"logical\" count=\"50\" version=\"1.0\">" +
                                "<entity name=\"fib_bitacoradisposicioneslinea\">" +
                                    "<attribute name=\"fib_cantidaddispuesta\" />" +
                                    "<attribute name=\"fib_creditoempresarial\" />" +
                                    "<attribute name=\"fib_disposicion\" />" +
                                    "<attribute name=\"fib_entramite\" />" +
                                    "<attribute name=\"fib_fechadedisposicion\" />" +
                                    "<attribute name=\"fib_notificousuario\" />" +
                                    "<attribute name=\"fib_periodosgracia\" />" +
                                    "<attribute name=\"fib_plazo\" />" +
                                    "<attribute name=\"fib_tipodecalculoid\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_cliente\" operator=\"eq\" value=\"" + numeroDeCliente + "\" />" +
                                    "</filter>" +
                                    "<link-entity name=\"fib_catalogodecalculodepago\" from=\"fib_catalogodecalculodepagoid\" to=\"fib_tipodecalculoid\" alias=\"catalogodecalculodepago\">" +
                                        "<attribute name=\"fib_codigo\" />" +
                                    "</link-entity>" +
                                "</entity>" +
                            "</fetch>";

            try
            {
                var service = CRMService.createService();
                var results = service.RetrieveMultiple(new FetchExpression(fetchXml));

                if (results.Entities.Any())
                {
                    foreach (var result in results.Entities)
                    {
                        BitacoraDisposicion bitacora = new BitacoraDisposicion();
                        bitacora.cantidadDispuesta = result.AtributoColleccion("fib_cantidaddispuesta").ToDecimal();
                        bitacora.fechaDisposicion = result.AtributoColleccion("fib_fechadedisposicion").ToStringNull();
                        bitacora.plazo = (int)result.AtributoColleccion("fib_plazo").ToInt32();
                        bitacora.tipoCalculo = result.AtributoColleccion("catalogodecalculodepago.fib_codigo").ToStringNull();

                        bitacora.enTramite = result.AtributoColleccion("fib_entramite").ToBoolean();

                        bitacora.notificoUsuario = result.AtributoColleccion("fib_notificousuario").ToBoolean();

                        bitacora.creditoEmpresarial = result.AtributoColleccion("fib_creditoempresarial").ToBoolean();
                        bitacora.periodosGracia = (int)result.AtributoColleccion("fib_periodosgracia").ToInt32();
                        if (result.AtributoColleccion("fib_disposicion") != null)
                        {
                            bitacora.numeroDisposicion = result.AtributoColleccion("fib_disposicion").ToStringNull();
                        }

                        bitacoras.Add(bitacora);
                    }
                }

            }
            catch (Exception)
            {
                throw new ConectorCRMException("FB-DL 4: Error al recuperar el registro de disposiciones del cliente " + numeroDeCliente);
            }

            return bitacoras;
        }

        public static DisposicionResponse CrearBitacoraDisposicion(String noCliente,string lineaDeCredito, decimal montoDispuesto, bool notificoUsuario, int plazo, DateTime fechaDisposicion, String tipoCalculo, int periodosGracia, string numeroSerie, string modeloDes, string marcaDes, string tipodeAutomovilDes,string color,string version, string noMotor,string descripcion)
        {
            String clienteId = getClienteId(noCliente);
            List<Configurador> configuradores = getConfiguradores(clienteId);
            var configurador = configuradores.Where(c => c.lineaCredito.Equals(lineaDeCredito)).First();
            if(configurador == null)
            {
                throw new Exception("No se encontro configuracion para la linea de credito");
            }
            String numeroDisposicion = "";

            // Crear el registro de la Bitácora (fib_bitacoradisposicioneslinea)
            Entity bitacora = new Entity("fib_bitacoradisposicioneslinea");

            // Set Cliente (fib_cliente)

            bitacora.Attributes.Add("fib_cliente", noCliente);
            String cliente_name = null;

            if (configurador.personaMoralId != null)
            {
                cliente_name = configurador.personaMoralName;
            }
            else if (configurador.personaFisicaId != null)
            {
                cliente_name = configurador.personaFisicaName;
            }
            else
            {
                throw new Exception("No se encontro un cliente fisico o moral ligado a la configuracion de cliente disposiciones en linea");
            }

            bitacora.Attributes.Add("fib_name", descripcion);

            // Set Configurador (fib_configuradorid)
            bitacora.Attributes.Add("fib_configuradorid", new EntityReference("fib_configclientesdisplinea", new Guid(configurador.id)));

            // Set Cantidad Dispuesta (fib_cantidaddispuesta)
            bitacora.Attributes.Add("fib_cantidaddispuesta", montoDispuesto);

            // Set Plazo (fib_plazo)
            bitacora.Attributes.Add("fib_plazo", plazo.ToString());

            // Set Fecha de Disposición (fib_fechadedisposicion)
            bitacora.Attributes.Add("fib_fechadedisposicion", fechaDisposicion);

            // Notificó al Usuario (fib_notificousuario)
            bitacora.Attributes.Add("fib_notificousuario", notificoUsuario);

            // Set Tipo de Cálculo (fib_tipodecalculoid)
            String tipodecalculoid = getTipoCalculoId(tipoCalculo);

            bitacora.Attributes.Add("fib_tipodecalculoid", new EntityReference("fib_catalogodecalculodepago", new Guid(tipodecalculoid)));

            // Períodos de Gracia (fib_periodosgracia)
            bitacora.Attributes.Add("fib_periodosgracia", periodosGracia);

            // En Trámite (fib_entramite) = true, indicando que el trámite de la disposición ha iniciado,
            // la bitacora dejará de estar en trámite al finalizar correctamente el proceso automático de disposición
            bitacora.Attributes.Add("fib_entramite", true);
            DisposicionResponse disp = null;
            // obtener datos de la garantia
            EntityReference marca = null;
            EntityReference tipoAutomovil = null;
            EntityReference concesionaria = null;
            Marca _marca = null;
            string descripcionGarantia = descripcion.Trim()
                + " / " + marcaDes.Trim()
                + (tipodeAutomovilDes != null ? " / " + tipodeAutomovilDes.Trim() : "")
                + " / " + modeloDes.Trim()
                + (color!= null ? " / " + color.Trim() : "");
            if (marcaDes != null)
            {
                _marca = getMarca(marcaDes);
                if (_marca != null)
                {
                    marca = new EntityReference("fib_marcaauto", new Guid(_marca.Id));
                }
                else
                {
                    throw new Exception("No se encontro la marca con nombre '"+ marcaDes + "' ");
                }
            }
            
            if (tipodeAutomovilDes != null)
            {
                var _tipoAutomovil = getTipoAutomovil(tipodeAutomovilDes);
                if (_tipoAutomovil != null)
                {
                    tipoAutomovil = new EntityReference("fib_tipoautomvil", new Guid(_tipoAutomovil.Id));
                }
                else
                {
                    throw new Exception("No se encontro la tipo de de automovil con nombre '" + tipodeAutomovilDes + "' ");
                }
            }
            
            
            //fib_clientepmid fib_clientepfid
            var _concesionaria = getConcesionaria(cliente_name);
            if (_concesionaria != null)
            {
                concesionaria = new EntityReference("fib_concesionaria", new Guid(_concesionaria.Id));
            }
            else
            {
                throw new Exception("No se encontro la concesionaria con nombre '" + cliente_name + "' ");
            }

            try
            {
                var service = CRMService.createService();
                var createResponse = service.Create(bitacora);

                //Modificación por adecuaciones a la bitácora
                Guid idNewBitacora = new Guid(createResponse.ToString());
                //Modificaciones por adecuaciones a la bitácora
                //Se llama al método que realizará la actualización del campo enTramite para disparar el plug-in
                updateBitacora(idNewBitacora);

                numeroDisposicion = getDisposicionByBitacora(idNewBitacora);
                Disposicion disposicion = getDisposicion(numeroDisposicion);
                //CREAR GARANTIA.

                Entity garantia = new Entity("fib_garantiadelineadecredito");
                garantia.Attributes.Add("fib_disposicionid", new EntityReference("fib_credito", new Guid(disposicion.Id)));
                garantia.Attributes.Add("fib_garantasid", new EntityReference("fib_lineadecredito", new Guid(disposicion.LineaCreditoId)));
                garantia.Attributes.Add("fib_monedaid", new EntityReference("fib_catalogodemoneda", new Guid(disposicion.MonedaId)));
                garantia.Attributes.Add("fib_valorgarantia_general", disposicion.Monto);
                garantia.Attributes.Add("fib_valorejecucion", disposicion.Monto);
                garantia.Attributes.Add("fib_noserie", numeroSerie);// numeroSerie);
                garantia.Attributes.Add("fib_tipoautomovilid", tipoAutomovil); // new EntityReference("fib_tipoautomovil",));
                garantia.Attributes.Add("fib_modelo", modeloDes); // new EntityReference("fib_modelo", Guid.Empty));
                garantia.Attributes.Add("fib_marcaid", marca); // new EntityReference("fib_marca", Guid.Empty));
                garantia.Attributes.Add("fib_garantia_text", Constantes.TIPO_GARANTIA_PRENDARIA);
                garantia.Attributes.Add("fib_tipodegarantia_text", "e2a4488f-52dd-df11-be72-005056972ca5");
                garantia.Attributes.Add("fib_name", descripcionGarantia.Trim());
                garantia.Attributes.Add("fib_descripcionunidad", descripcionGarantia.Trim());
                garantia.Attributes.Add("fib_concesionariaid", concesionaria);
                if (color != null)
                {
                    garantia.Attributes.Add("fib_color", color);
                }
                garantia.Attributes.Add("fib_version", version);

                var garantiaResponse = service.Create(garantia);
                Guid idGarantia = new Guid(createResponse.ToString());
                // Fin de modificaciones por adecuaciones a la bitácora
                string numeroGarantia = getNumeroGarantiaById(idGarantia.ToString());
                String comision = getComision(numeroDisposicion);
                disp = new DisposicionResponse();
                disp.numeroDisposicion = numeroDisposicion;
                disp.result = true;
                disp.comisionEIva = comision;
                disp.numeroGarantia = numeroGarantia;
            }
            catch (ConectorCRMException ex)
            {
                disp = new DisposicionResponse();
                disp.result = false;
                disp.error = ex.Message;
            }
            catch (Exception ex)
            {
                disp = new DisposicionResponse();
                disp.result = false;
                disp.error = ex.Message;
            }

            return disp;
        }

        public static List<Credito> ObtenerDetalleCreditos(string numeroCliente, string linea, string credito)
        {
            ConectorAX conectorAx = new ConectorAX();
            conectorAx.Logon();
            var ax = conectorAx.ax;
            object[] paramlist = { numeroCliente,linea,credito };
            string result = (string)ax.CallStaticClassMethod("AND_MicorservicesInterface", "getConsultaClientesFIB", paramlist);
            string json = result.Replace("<data>", "").Replace("</data>", "");
            var listaCreditos = Newtonsoft.Json.JsonConvert.DeserializeObject<CreditoList>(json);
            conectorAx.Logoff();
            return listaCreditos.creditos;
        }

        public static List<Aval> ObtenerAvalesPorLineaDeCredito(string lineaDeCredito)
        {
            List<Aval> customers = new List<Aval>();

            var service = CRMService.createService();
            String fetchXml = "<fetch mapping=\"logical\" version=\"1.0\">" +
                                "<entity name=\"fib_lineadecredito\">" +
                                    "<filter>" +
                                            "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                            "<condition attribute=\"fib_name\" operator=\"eq\" value=\"{0}\" />" +
                                    "</filter>" +
                                    "<link-entity name=\"opportunity\" from=\"opportunityid\" to=\"fib_solicituddecreditoid\" link-type=\"inner\" alias=\"opportunity\">" +
                                    "<filter>" +
                                            "<condition attribute=\"statecode\" operator=\"in\" >" +
                                                "<value>0</value>" +
                                                "<value>1</value>" +
                                             "</condition>" +
                                    "</filter>" +
                                        "<link-entity name=\"fib_aval\" from=\"fib_opportunityid\" to=\"opportunityid\" link-type=\"inner\" alias=\"aval\">" +
                                        "<filter>" +
                                            "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                        "</filter>" +
                                            "<link-entity name=\"account\" from=\"accountid\" to=\"fib_avalpmid\" link-type=\"outer\" alias=\"account\">" +
                                            "<attribute name=\"accountnumber\" />" +
                                            "<attribute name=\"name\" />" +
                                            "<attribute name=\"fib_nombrecomercial\" />" +
                                            "<attribute name=\"fib_rfc\" />" +
                                            "<attribute name=\"fib_email\" />" +
                                            "<attribute name=\"fib_coloniaid\" />" +
                                            "<attribute name=\"address1_line1\" />" +
                                            "<attribute name=\"address1_postalcode\" />" +
                                            "<attribute name=\"fib_municipioid\" />" +
                                            "<attribute name=\"new_estadoid\" />" +
                                            "<attribute name=\"telephone1\" />" +
                                            "<filter>" +
                                                "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                            "</filter>" +
                                            "</link-entity>" +
                                            "<link-entity name=\"contact\" from=\"contactid\" to=\"fib_avalpfid\" link-type=\"outer\" alias=\"contact\">" +
                                                "<attribute name=\"fib_numpersonafisica\" />" +
                                                "<attribute name=\"firstname\" />" +
                                                "<attribute name=\"middlename\" />" +
                                                "<attribute name=\"lastname\" />" +
                                                "<attribute name=\"fib_apellidomaterno\" />" +
                                                "<attribute name=\"fib_rfc\" />" +
                                                "<attribute name=\"emailaddress1\" />" +
                                                "<attribute name=\"telephone1\" />" +
                                                "<attribute name=\"address1_line1\" />" +
                                                "<attribute name=\"fib_coloniaid\" />" +
                                                "<attribute name=\"address1_postalcode\" />" +
                                                "<attribute name=\"fib_delegmposid\" />" +
                                                "<attribute name=\"new_estadoid\" />" +
                                                "<filter>" +
                                                    "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                                "</filter>" +
                                            "</link-entity>" +
                                        "</link-entity>" +
                                    "</link-entity>" +
                                "</entity>" +
                            "</fetch>";

            var results = service.RetrieveMultiple(new FetchExpression(string.Format(fetchXml,lineaDeCredito)));
            if (results.Entities.Any())
            {
                foreach (var result in results.Entities)
                {
                    if (result.Attributes.Contains("account.accountnumber"))
                    {
                        string colonia = result.AtributoColleccion("account.fib_coloniaid", TipoAtributos.ENTITY_REFERENCE_NAME).ToStringNull();
                        string municipio = result.AtributoColleccion("account.fib_municipioid", TipoAtributos.ENTITY_REFERENCE_NAME).ToStringNull();
                        string estado = result.AtributoColleccion("account.new_estadoid", TipoAtributos.ENTITY_REFERENCE_NAME).ToStringNull();
                        customers.Add(new Aval
                        {
                            numeroCliente = result.AtributoColleccion("account.accountnumber").ToStringNull(),
                            nombres = (result.AtributoColleccion("account.name").ToStringNull() != null ? (result.AtributoColleccion("account.name").ToString() + " ") : "").Trim(),
                            apellidos = "",//((result.AtributoColleccion("account.lastname").ToStringNull() != null ? (result.AtributoColleccion(alias + ".lastname").ToString() + " ") : "") +
                                    //(result.AtributoColleccion("account.fib_apellidomaterno").ToStringNull() != null ? (result.AtributoColleccion(alias + ".fib_apellidomaterno").ToString()) : "")).Trim(),
                            rfc = result.AtributoColleccion("account.fib_rfc").ToStringNull(),
                            correo = result.AtributoColleccion("account.fib_email").ToStringNull(),
                            direccionFiscal = result.AtributoColleccion("account.address1_line1").ToStringNull() != null ? (result.AtributoColleccion( "account.address1_line1").ToStringNull() +
                            " COLONIA " + (colonia != null ? (colonia.Contains("-") ? colonia.Split('-')[1].Trim() : colonia) : "") + ", " +
                            "CÓDIGO POSTAL " + (result.AtributoColleccion("account.address1_postalcode").ToStringNull()) + ", " +
                            (municipio != null ? (municipio.Contains("-") ? municipio.Split('-')[1].Trim() : municipio) : "") + " " +
                            (estado != null ? (estado.Contains("-") ? estado.Split('-')[1].Trim() : estado) : "")) : null,
                            telefono = result.AtributoColleccion("account.telephone1").ToStringNull()
                        });
                    }
                    if (result.Attributes.Contains("contact.fib_numpersonafisica"))
                    {
                        string colonia = result.AtributoColleccion("contact.fib_coloniaid", TipoAtributos.ENTITY_REFERENCE_NAME).ToStringNull();
                        string municipio = result.AtributoColleccion("contact.fib_delegmposid", TipoAtributos.ENTITY_REFERENCE_NAME).ToStringNull();
                        string estado = result.AtributoColleccion("contact.new_estadoid", TipoAtributos.ENTITY_REFERENCE_NAME).ToStringNull();
                        customers.Add(new Aval
                        {
                            numeroCliente = result.AtributoColleccion("contact.fib_numpersonafisica").ToStringNull(),
                            nombres = ((result.AtributoColleccion("contact.firstname").ToStringNull() != null ? (result.AtributoColleccion("contact.firstname").ToString() + " ") : "") +
                                    (result.AtributoColleccion("contact.middlename").ToStringNull() != null ? (result.AtributoColleccion("contact.middlename").ToString()) : "")).Trim(),
                            apellidos = ((result.AtributoColleccion("contact.lastname").ToStringNull() != null ? (result.AtributoColleccion("contact.lastname").ToString() + " ") : "") +
                                    (result.AtributoColleccion("contact.fib_apellidomaterno").ToStringNull() != null ? (result.AtributoColleccion("contact.fib_apellidomaterno").ToString()) : "")).Trim(),
                            rfc = result.AtributoColleccion("contact.fib_rfc").ToStringNull(),
                            correo = result.AtributoColleccion("contact.emailaddress1").ToStringNull(),
                            direccionFiscal = result.AtributoColleccion("contact.address1_line1").ToStringNull() != null ? (result.AtributoColleccion("contact.address1_line1").ToStringNull() +
                            " COLONIA " + (colonia != null ? (colonia.Contains("-") ? colonia.Split('-')[1].Trim() : colonia) : "") + ", " +
                            "CÓDIGO POSTAL " + (result.AtributoColleccion("contact.address1_postalcode").ToStringNull()) + ", " +
                            (municipio != null ? (municipio.Contains("-") ? municipio.Split('-')[1].Trim() : municipio) : "") + " " +
                            (estado != null ? (estado.Contains("-") ? estado.Split('-')[1].Trim() : estado) : "")) : null,
                            telefono = result.AtributoColleccion("contact.telephone1").ToStringNull()
                        });
                    }
                }
            }

            return customers;
        }

        public static List<Cliente> getClientes(string filtro)
        {
            List<Cliente> clientes = new List<Cliente>();
            if(filtro != null && filtro.Trim().Length > 0)
            {
                String[] filtros = filtro.Split(' ');
                String filtroNombrePF = "";
                String filtroApellidoMaterno = "";
                String filtroNombrePM = "";
                foreach(String filtroStr in filtros){
                    if (filtroStr.Trim().Length == 0) continue;
                    filtroNombrePF += "<condition attribute=\"fullname\" operator=\"like\" value=\"%" + filtroStr + "%\" />";
                    filtroNombrePM += "<condition attribute=\"name\" operator=\"like\" value=\"%" + filtroStr + "%\" />";
                    filtroApellidoMaterno += "<condition attribute=\"fib_apellidomaterno\" operator=\"like\" value=\"%" + filtroStr + "%\" />";
                }
                
                String fetchXmlFisica = "<fetch mapping=\"logical\" version=\"1.0\">" +
                                   "<entity name=\"contact\">" +
                                       "<attribute name=\"firstname\" />" +
                                       "<attribute name=\"middlename\" />" +
                                       "<attribute name=\"lastname\" />" +
                                       "<attribute name=\"fib_apellidomaterno\" />" +
                                       "<attribute name=\"fib_rfc\" />" +
                                       "<attribute name=\"fib_numpersonafisica\" />" +
                                       "<filter type=\"or\">" + 
                                           "<condition attribute=\"fib_rfc\" operator=\"like\" value=\"%" + filtro + "%\" />" +
                                         "<filter type=\"and\">" +
                                            filtroNombrePF +
                                         "</filter>" +
                                         
                                       "</filter>" +
                                   "</entity>" +
                               "</fetch>";
                String fetchXmlMoral = "<fetch mapping=\"logical\" version=\"1.0\">" +
                                    "<entity name=\"account\">" +
                                        "<attribute name=\"name\" />" +
                                        "<attribute name=\"fib_rfc\" />" +
                                        "<attribute name=\"accountnumber\" />" +
                                        "<filter type=\"or\">" +
                                            "<condition attribute=\"fib_rfc\" operator=\"like\" value=\"%" + filtro + "%\" />" +
                                            "<filter type=\"and\">" +
                                            filtroNombrePM +
                                            "</filter>" +
                                        "</filter>" +
                                    "</entity>" +
                                "</fetch>";

                try
                {
                    var service = CRMService.createService();
                    var results = service.RetrieveMultiple(new FetchExpression(fetchXmlFisica));

                    if (results.Entities.Any())
                    {
                        foreach (var result in results.Entities)
                        {
                            clientes.Add(new Cliente
                            {
                                codigoCliente = result.AtributoColleccion("fib_numpersonafisica").ToStringNull(),
                                nombre = (result.AtributoColleccion("firstname").ToStringNull() != null ? (result.AtributoColleccion("firstname").ToString() + " ") : "") +
                                    (result.AtributoColleccion("middlename").ToStringNull() != null ? (result.AtributoColleccion("middlename").ToString() + " ") : "") +
                                    (result.AtributoColleccion("lastname").ToStringNull() != null ? (result.AtributoColleccion("lastname").ToString() + " ") : "") +
                                    (result.AtributoColleccion("fib_apellidomaterno").ToStringNull() != null ? (result.AtributoColleccion("fib_apellidomaterno").ToString() + " ") : "")
                                ,
                                rfc = result.AtributoColleccion("fib_rfc").ToStringNull()
                            });
                        }
                    }

                    results = service.RetrieveMultiple(new FetchExpression(fetchXmlMoral));

                    if (results.Entities.Any())
                    {
                        foreach (var result in results.Entities)
                        {
                            clientes.Add(new Cliente
                            {
                                codigoCliente = result.AtributoColleccion("accountnumber").ToStringNull(),
                                nombre = result.AtributoColleccion("name").ToStringNull(),
                                rfc = result.AtributoColleccion("fib_rfc").ToStringNull()
                            });
                        }
                    }

                }
                catch (Exception)
                {
                    throw new ConectorCRMException("FB-DL 4: Error al recuperar el registro de los clientes [" + filtro + "]");
                }
            }

            return clientes;
        }

        public static List<CuentaCorreo> getClientesCorreo(string correo)
        {
            List<CuentaCorreo> clientes = new List<CuentaCorreo>();
            if (correo != null && correo.Trim().Length > 0)
            {

                String fetchXmlFisica = "<fetch mapping=\"logical\" version=\"1.0\">" +
                                   "<entity name=\"contact\">" +
                                       "<attribute name=\"firstname\" />" +
                                       "<attribute name=\"middlename\" />" +
                                       "<attribute name=\"lastname\" />" +
                                       "<attribute name=\"fib_apellidomaterno\" />" +
                                       "<attribute name=\"fib_rfc\" />" +
                                       "<attribute name=\"fib_numpersonafisica\" />" +
                                       "<filter>" +
                                           "<condition attribute=\"emailaddress1\" operator=\"eq\" value=\"" + correo + "\" />" +
                                       "</filter>" +
                                   "</entity>" +
                               "</fetch>";
                String fetchXmlMoral = "<fetch mapping=\"logical\" version=\"1.0\">" +
                                    "<entity name=\"account\">" +
                                        "<attribute name=\"name\" />" +
                                        "<attribute name=\"fib_rfc\" />" +
                                        "<attribute name=\"accountnumber\" />" +
                                        "<filter>" +
                                            "<condition attribute=\"fib_email\" operator=\"eq\" value=\"" + correo + "\" />" +
                                        "</filter>" +
                                    "</entity>" +
                                "</fetch>";

                String fetchXmlOpportunityFisica = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                   "<entity name=\"opportunity\">" +
                                       "<filter>" +
                                           "<condition attribute=\"contactid\" operator=\"eq\" value=\"{0}\" />" +
                                       "</filter>" +
                                   "</entity>" +
                               "</fetch>";

                String fetchXmlOpportunityMoral = "<fetch mapping=\"logical\" count=\"1\"  version=\"1.0\">" +
                                    "<entity name=\"opportunity\">" +
                                        "<filter>" +
                                            "<condition attribute=\"accountid\" operator=\"eq\" value=\"{0}\" />" +
                                        "</filter>" +
                                    "</entity>" +
                                "</fetch>";

                try
                {
                    var service = CRMService.createService();
                    var results = service.RetrieveMultiple(new FetchExpression(fetchXmlFisica));

                    if (results.Entities.Any())
                    {
                        foreach (var result in results.Entities)
                        {
                            clientes.Add(new CuentaCorreo
                            {
                                NumeroCuenta = result.AtributoColleccion("fib_numpersonafisica").ToStringNull(),
                                NombreORazonSocial = (result.AtributoColleccion("firstname").ToStringNull() != null ? (result.AtributoColleccion("firstname").ToString() + " ") : "") +
                                    (result.AtributoColleccion("middlename").ToStringNull() != null ? (result.AtributoColleccion("middlename").ToString() + " ") : "") +
                                    (result.AtributoColleccion("lastname").ToStringNull() != null ? (result.AtributoColleccion("lastname").ToString() + " ") : "") +
                                    (result.AtributoColleccion("fib_apellidomaterno").ToStringNull() != null ? (result.AtributoColleccion("fib_apellidomaterno").ToString() + " ") : ""),
                                ClienteId = result.Id,
                                PersonaFisica = true
                            });
                        }
                    }

                    results = service.RetrieveMultiple(new FetchExpression(fetchXmlMoral));

                    if (results.Entities.Any())
                    {
                        foreach (var result in results.Entities)
                        {
                            clientes.Add(new CuentaCorreo
                            {
                                NumeroCuenta = result.AtributoColleccion("accountnumber").ToStringNull(),
                                NombreORazonSocial = result.AtributoColleccion("name").ToStringNull(),
                                ClienteId = result.Id,
                                PersonaFisica = false
                            });
                        }
                    }

                    foreach(var cliente in clientes)
                    {
                        if (cliente.PersonaFisica)
                        {
                            results = service.RetrieveMultiple(new FetchExpression(string.Format(fetchXmlOpportunityFisica,cliente.ClienteId.ToString())));
                        }
                        else
                        {
                            results = service.RetrieveMultiple(new FetchExpression(string.Format(fetchXmlOpportunityMoral, cliente.ClienteId.ToString())));
                        }
                        if (results.Entities.Any())
                        {
                            cliente.Creditos = true;
                        }
                    }


                }
                catch (Exception)
                {
                    throw new ConectorCRMException("FB-DL 4: Error al recuperar el registro de las cuentas del correo [" + correo + "]");
                }
            }

            return clientes.Where(c=>c.Creditos).ToList();
        }

        #region PRIVATE_METHODS

        private static Boolean tieneExigible(String noCliente)
        {
            Boolean exigible = false;

            try
            {
                String fetchXml = "<fetch mapping=\"logical\" count=\"50\" version=\"1.0\">" +
                                "<entity name=\"fib_bitacoradisposicioneslinea\">" +
                                    "<attribute name=\"fib_disposicion\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_cliente\" operator=\"eq\" value=\"" + noCliente + "\" />" +
                                        "<condition attribute=\"fib_disposicion\" operator=\"not-null\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";

                List<string> disposiciones = new List<string>();
                var service = CRMService.createService();
                var results = service.RetrieveMultiple(new FetchExpression(fetchXml));

                if (results.Entities.Any())
                {
                    foreach (var result in results.Entities)
                    {
                        disposiciones.Add(result.AtributoColleccion("fib_disposicion").ToStringNull());
                    }
                }


                if (disposiciones.Any())
                {
                    var disposicionesStr = "<value>" + String.Join("</value><value>", disposiciones) + "</value>";
                    fetchXml = "<fetch mapping=\"logical\" count=\"50\" version=\"1.0\">" +
                            "<entity name=\"fib_credito\">" +
                                "<filter>" +
                                    "<condition attribute=\"fib_codigo\" operator=\"in\">" +
                                        disposicionesStr +
                                    "</condition>" +
                                "</filter>" +
                                "<link-entity name=\"fib_seguimientodecobranza\" from=\"fib_disposiciondecreditoid\" to=\"fib_creditoid\">" +
                                    "<attribute name=\"fib_interesexigible\" />" +
                                    "<attribute name=\"fib_saldoexigible\" />" +
                                    "<attribute name=\"transactioncurrencyid\" />" +
                                    "<filter type=\"or\">" +
                                        "<condition attribute=\"fib_capitalexigible\" operator=\"gt\" value=\"0\" />" +
                                        "<condition attribute=\"fib_interesexigible\" operator=\"gt\" value=\"0\" />" +
                                    "</filter>" +
                                "</link-entity>" +
                            "</entity>" +
                        "</fetch>";

                    results = service.RetrieveMultiple(new FetchExpression(fetchXml));
                    if (results.Entities.Any())
                    {
                        exigible = true;
                    }
                }
            }
            catch (Exception)
            {
                throw new ConectorCRMException("FB-DL 4: Error al recuperar el registro de disposiciones del cliente " + noCliente);
            }

            return exigible;
        }

        private static String getClienteId(String noCliente)
        {
            String prefijo = noCliente.Substring(0, 2);
            String fetchXml = null;
            String clienteId = null;


            if (prefijo.Equals("PM"))
            {
                fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                            "<entity name=\"account\">" +
                                "<attribute name=\"accountid\" />" +
                                "<filter>" +
                                    "<condition attribute=\"accountnumber\" operator=\"eq\" value=\"" + noCliente + "\" />" +
                                "</filter>" +
                            "</entity>" +
                        "</fetch>";
            }
            else
            {
                fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                            "<entity name=\"contact\">" +
                                "<attribute name=\"contactid\" />" +
                                "<filter>" +
                                    "<condition attribute=\"fib_numpersonafisica\" operator=\"eq\" value=\"" + noCliente + "\" />" +
                                "</filter>" +
                            "</entity>" +
                        "</fetch>";
            }

            try
            {
                var service = CRMService.createService();
                var results = service.RetrieveMultiple(new FetchExpression(fetchXml));

                if (results.Entities.Any())
                {
                    foreach (var result in results.Entities)
                    {
                        if (prefijo.Equals("PM"))
                            clienteId = result.AtributoColleccion("accountid").ToStringNull();
                        else
                            clienteId = result.AtributoColleccion("contactid").ToStringNull();
                    }
                }


            }
            catch (Exception)
            {
                throw new ConectorCRMException("FB-CRM 1: Error al recuperar información del cliente " + noCliente);
            }

            return clienteId;
        }

        private static List<Configurador> getConfiguradores(String clienteId, string tipo = "")
        {
            List<Configurador> configuradores = new List<Configurador>();
            String filtro = "<condition attribute=\"fib_tipo\" operator=\"eq\" value=\"{0}\" />";
            if (tipo.Trim().Any())
            {
                filtro = string.Format(filtro, tipo);
            }
            else
            {
                filtro = "";
            }
            String fetchXml = "<fetch mapping=\"logical\" version=\"1.0\">" +
                                "<entity name=\"fib_configclientesdisplinea\">" +
                                    "<attribute name=\"fib_bancoid\" />" +
                                    "<attribute name=\"fib_comision\" />" +
                                    "<attribute name=\"fib_clabe\" />" +
                                    "<attribute name=\"fib_cuenta\" />" +
                                    "<attribute name=\"fib_clientepmid\" />" +
                                    "<attribute name=\"fib_clientepfid\" />" +
                                    "<attribute name=\"fib_configclientesdisplineaid\" />" +
                                    "<attribute name=\"fib_impuesto\" />" +
                                    "<attribute name=\"fib_plazosdepagonombres\" />" +
                                    "<attribute name=\"fib_tasadeinteres\" />" +
                                    "<attribute name=\"fib_tasamoratoria\" />" +
                                    "<attribute name=\"fib_tasavariable\" />" +
                                    "<attribute name=\"fib_operador\" />" +
                                    "<attribute name=\"fib_margen\" />" +
                                    "<attribute name=\"fib_diadecorte\" />" +
                                    "<attribute name=\"fib_tipo\" />" +
                                    "<attribute name=\"fib_diacambiomes\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_cliente\" operator=\"like\" value=\"%" + clienteId + "%\" />" +
                                        filtro +
                                    "</filter>" +
                                    "<link-entity name=\"fib_tasa\" from=\"fib_tasaid\" to=\"fib_tasavariable\" link-type=\"outer\" alias=\"tasa\">" +
                                        "<attribute name=\"fib_name\" />" +
                                        "<filter>" +
                                            "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                            "</filter>" +
                                    "</link-entity>" +
                                    "<link-entity name=\"fib_lineadecredito\" from=\"fib_configuradorid\" to=\"fib_configclientesdisplineaid\" alias=\"linea\">" +
                                        "<attribute name=\"fib_creditoempresarial\" />" +
                                        "<attribute name=\"fib_fechavencimiento\" />" +
                                        "<attribute name=\"fib_importedisponible\" />" +
                                        "<attribute name=\"fib_lineadecreditoid\" />" +
                                        "<attribute name=\"transactioncurrencyid\" />" +
                                        "<attribute name=\"fib_estatus\" />" +
                                        "<attribute name=\"fib_name\" />" +
                                        "<filter>" +
                                            "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                        "</filter>" +
                                    "</link-entity>" +
                                "</entity>" +
                            "</fetch>";

            try
            {
                var service = CRMService.createService();
                var results = service.RetrieveMultiple(new FetchExpression(fetchXml));

                if (results.Entities.Any())
                {
                    foreach (var result in results.Entities)
                    {
                        var _tasafijavariable = result.AtributoColleccion("fib_tasavariable",TipoAtributos.ENTITY_REFERENCE_NAME).ToStringNull();
                        configuradores.Add(new Configurador
                        {
                            id = result.AtributoColleccion("fib_configclientesdisplineaid").ToStringNull(),
                            bancoId = result.AtributoColleccion("fib_bancoid", TipoAtributos.ENTITY_REFERENCE_ID).ToStringNull(),
                            bancoName = result.AtributoColleccion("fib_bancoid", TipoAtributos.ENTITY_REFERENCE_NAME).ToStringNull(),
                            clabe = result.AtributoColleccion("fib_clabe").ToStringNull(),
                            cuenta = result.AtributoColleccion("fib_cuenta").ToStringNull(),
                            comision = result.AtributoColleccion("fib_comision").ToStringNull(),
                            personaFisicaId = result.AtributoColleccion("fib_clientepfid", TipoAtributos.ENTITY_REFERENCE_ID).ToStringNull(),
                            personaFisicaName = result.AtributoColleccion("fib_clientepfid", TipoAtributos.ENTITY_REFERENCE_NAME).ToStringNull(),
                            personaMoralId = result.AtributoColleccion("fib_clientepmid", TipoAtributos.ENTITY_REFERENCE_ID).ToStringNull(),
                            personaMoralName = result.AtributoColleccion("fib_clientepmid", TipoAtributos.ENTITY_REFERENCE_NAME).ToStringNull(),
                            impuesto = result.AtributoColleccion("fib_impuesto").ToInt32(),
                            plazosDePagoNombres = result.AtributoColleccion("fib_plazosdepagonombres").ToStringNull(),
                            diaDeCorte = result.AtributoColleccion("fib_diadecorte").ToInt32(),
                            creditoEmpresarial = result.AtributoColleccion("linea.fib_creditoempresarial").ToBoolean(),
                            fechaVencimiento = (DateTime)result.AtributoColleccion("linea.fib_fechavencimiento"),
                            importeDisponible = result.AtributoColleccion("linea.fib_importedisponible", TipoAtributos.MONEY).ToDecimal(),
                            lineaDeCreditoId = result.AtributoColleccion("linea.fib_lineadecreditoid").ToStringNull(),
                            transaccionCurrencyId = result.AtributoColleccion("linea.transactioncurrencyid", TipoAtributos.ENTITY_REFERENCE_ID).ToStringNull(),
                            transaccionCurrencyName = result.AtributoColleccion("linea.transactioncurrencyid", TipoAtributos.ENTITY_REFERENCE_NAME).ToStringNull(),
                            status = result.AtributoColleccion("linea.fib_estatus", TipoAtributos.OPCION).ToStringNull(),
                            tipoDispLinea = result.AtributoColleccion("fib_tipo", TipoAtributos.OPCION).ToString(),
                            lineaCredito = result.AtributoColleccion("linea.fib_name").ToString(),
                            tasaFijaVariable = _tasafijavariable,
                            tasaMora = result.AtributoColleccion("fib_tasamoratoria").ToDecimal(),
                            tasaInteres = _tasafijavariable.Equals(Constantes.TASA_FIJA) ?
                                    result.AtributoColleccion("fib_tasadeinteres").ToStringNull() :
                                    result.AtributoColleccion("tasa.fib_name").ToStringNull() + result.AtributoColleccion("fib_operador", TipoAtributos.OPCION_TEXTO).ToStringNull() + result.AtributoColleccion("fib_margen").ToStringNull(),
                            valorTasaVariable = result.AtributoColleccion("fib_tasadeinteres").ToDecimal(),
                            diaCambioMes = result.AtributoColleccion("fib_diacambiomes").ToInt32()
                        });
                    }
                }
                else
                {
                    throw new ConectorCRMException("FB-CRM 3: El cliente no esta habilitado para realizar disposiciones en línea");
                }
            }
            catch (ConectorCRMException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ConectorCRMException("FB-CRM 3-1: Error al recuperar información del cliente");
            }
            configuradores = configuradores.Where(c => c.fechaVencimiento.CompareTo(DateTime.Today) > 0).ToList();
            return configuradores;
        }

        private static DisponiblePorProducto getDisponible(String lineadecreditoId)
        {
            DisponiblePorProducto disponiblePorProducto = new DisponiblePorProducto();

            String fetchXml = "<fetch mapping=\"logical\" count=\"50\" version=\"1.0\">" +
                                "<entity name=\"fib_disponibleporproducto\">" +
                                    "<attribute name=\"fib_importedisponible\" />" +
                                    "<attribute name=\"fib_importedispuesto\" />" +
                                    "<attribute name=\"fib_importerevolvente\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_lineadecreditoid\" operator=\"eq\" value=\"" + lineadecreditoId + "\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";

            try
            {
                var service = CRMService.createService();
                var results = service.RetrieveMultiple(new FetchExpression(fetchXml));

                if (results.Entities.Any())
                {
                    foreach (var result in results.Entities)
                    {
                        disponiblePorProducto = new DisponiblePorProducto
                        {
                            importeDisponible = result.AtributoColleccion("fib_importedisponible", TipoAtributos.MONEY).ToDecimal2(),
                            importeDispuesto = result.AtributoColleccion("fib_importedispuesto", TipoAtributos.MONEY).ToDecimal2(),
                            importeRevolvente = result.AtributoColleccion("fib_importerevolvente", TipoAtributos.MONEY).ToDecimal2()
                        };
                    }
                }
                else
                {
                    throw new ConectorCRMException("FB-CRM 5: El cliente no tiene monto disponible en la Línea de Crédito");
                }
            }
            catch (ConectorCRMException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ConectorCRMException("FB-CRM 5-1: Error al recuperar información del cliente");
            }

            return disponiblePorProducto;
        }

        private static String getImpuestoCliente(String noCliente)
        {
            String prefijo = noCliente.Substring(0, 2);
            String fetchXml = null;
            String impuesto = null;

            if (prefijo.Equals("PM"))
            {
                fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                "<entity name=\"account\">" +
                                    "<attribute name=\"fib_impuestoaplicable\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"accountnumber\" operator=\"eq\" value=\"" + noCliente.Trim().ToUpper() + "\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";
            }
            else
            {
                fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                    "<entity name=\"contact\">" +
                                        "<attribute name=\"fib_impuestosaplicables\" />" +
                                        "<filter>" +
                                            "<condition attribute=\"fib_numpersonafisica\" operator=\"eq\" value=\"" + noCliente.Trim().ToUpper() + "\" />" +
                                        "</filter>" +
                                    "</entity>" +
                                "</fetch>";
            }

            try
            {
                var service = CRMService.createService();
                var results = service.RetrieveMultiple(new FetchExpression(fetchXml));

                if (results.Entities.Any())
                {
                    foreach (var result in results.Entities)
                    {
                        if (prefijo.Equals("PM"))
                        {
                            impuesto = result.AtributoColleccion("fib_impuestoaplicable", TipoAtributos.OPCION) == null ? "4" : result.AtributoColleccion("fib_impuestoaplicable", TipoAtributos.OPCION).ToStringNull();
                        }
                        else
                        {
                            impuesto = result.AtributoColleccion("fib_impuestosaplicables", TipoAtributos.OPCION) == null ? "4" : result.AtributoColleccion("fib_impuestosaplicables", TipoAtributos.OPCION).ToStringNull();
                        }

                    }
                }
            }
            catch (Exception)
            {
                throw new ConectorCRMException("FB-CRM 1: Error al recuperar información del cliente " + noCliente);
            }

            return impuesto;
        }



        private static String getProducto(String lineacreditoId)
        {
            String producto = null;

            String fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                "<entity name=\"fib_lineadecredito\">" +
                                    "<attribute name=\"fib_name\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_lineadecreditoid\" operator=\"eq\" value=\"" + lineacreditoId + "\" />" +
                                    "</filter>" +
                                    "<link-entity name=\"fib_disponibleporproducto\" from=\"fib_lineadecreditoid\" to=\"fib_lineadecreditoid\" alias =\"disponibleporproducto\">" +
                                        "<attribute name=\"fib_disponibleporproductoid\" />" +
                                        "<order attribute=\"createdon\" descending=\"true\" />" +
                                        "<link-entity name=\"fib_producto\" from=\"fib_productoid\" to=\"fib_productoid\" alias=\"producto\">" +
                                            "<attribute name=\"fib_nodeproducto\" />" +
                                        "</link-entity>" +
                                    "</link-entity>" +
                                "</entity>" +
                            "</fetch>";

            try
            {
                var service = CRMService.createService();
                var results = service.RetrieveMultiple(new FetchExpression(fetchXml));

                if (results.Entities.Any())
                {
                    foreach (var result in results.Entities)
                    {
                        producto = result.AtributoColleccion("producto.fib_nodeproducto").ToStringNull();
                    }
                }
                else
                {
                    throw new ConectorCRMException("FB-CRM 3-2: No hay disponible en la Línea de Crédito del Cliente");
                }

            }
            catch (ConectorCRMException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ConectorCRMException("FB-CRM 3-3: Error al recuperar información del cliente");
            }

            return producto;
        }

        private static int MaxIntentosAutenticacion()
        {
            String fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                               "<entity name=\"fib_configuracion\">" +
                                   "<attribute name=\"fib_erroresautenticacion\" />" +
                               "</entity>" +
                           "</fetch>";

            var service = CRMService.createService();
            var results = service.RetrieveMultiple(new FetchExpression(fetchXml));

            if (results.Entities.Any())
            {
                return int.Parse(results.Entities[0].AtributoColleccion("fib_erroresautenticacion").ToString());
            }

            return 0;
        }

        private static String getTipoCalculoId(String codigo)
        {
            String id = null;

            String fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                "<entity name=\"fib_catalogodecalculodepago\">" +
                                    "<attribute name=\"fib_catalogodecalculodepagoid\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_codigo\" operator=\"eq\" value=\"" + codigo + "\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";

            try
            {
                var service = CRMService.createService();
                var results = service.RetrieveMultiple(new FetchExpression(fetchXml));


                id = results.Entities[0].AtributoColleccion("fib_catalogodecalculodepagoid").ToString();
            }
            catch
            {
                throw new ConectorCRMException("FB-CRM 4: Error al recuperar información del cliente");
            }

            return id;
        }

        private static void updateBitacora(Guid idNewBitacora)
        {
            var service = CRMService.createService();
            Entity bitacora = new Entity("fib_bitacoradisposicioneslinea");
            bitacora.Id = idNewBitacora;

            // Se actualiza el valor de fib_iniciarproceso para disparar el plugin
            bitacora.Attributes.Add("fib_iniciarproceso", true);

            service.Update(bitacora);
        }

        private static string getDisposicionByBitacora(Guid idNewBitacora)
        {
            String disposicion = "";
            Bitacora bitacoraInfo = getBitacoraById(idNewBitacora.ToString());

            if (bitacoraInfo != null)
                disposicion = (bitacoraInfo.disposicion != null) ? bitacoraInfo.disposicion : "";

            return disposicion;
        }

        private static Moneda getMoneda(String codigoMoneda)
        {
            Moneda moneda = null;

            String fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                "<entity name=\"fib_catalogodemoneda\">" +
                                    "<attribute name=\"fib_catalogodemonedaid\" />" +
                                    "<attribute name=\"fib_codigodemoneda\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_codigodemoneda\" operator=\"eq\" value=\"" + codigoMoneda + "\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";


            var service = CRMService.createService();
            var results = service.RetrieveMultiple(new FetchExpression(fetchXml));

            if (results.Entities.Any())
            {
                var entity = results.Entities[0];
                moneda = new Moneda();
                moneda.Id = entity.AtributoColleccion("fib_catalogodemonedaid").ToStringNull();
                moneda.Codigo = entity.AtributoColleccion("fib_codigodemoneda").ToStringNull();

            }
            else
            {
                throw new ConectorCRMException("FB-CRM : No se puede recuperar datos de la garantia");
            }

            return moneda;
        }

        private static Modelo getModelo(String name)
        {
            Modelo modelo = null;

            String fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                "<entity name=\"fib_modeloauto\">" +
                                    "<attribute name=\"fib_modeloautoid\" />" +
                                    "<attribute name=\"fib_name\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_name\" operator=\"like\" value=\"%" + name + "%\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";


            var service = CRMService.createService();
            var results = service.RetrieveMultiple(new FetchExpression(fetchXml));

            if (results.Entities.Any())
            {
                var entity = results.Entities[0];
                modelo = new Modelo();
                modelo.Id = entity.AtributoColleccion("fib_modeloautoid").ToStringNull();
                modelo.Name = entity.AtributoColleccion("fib_name").ToStringNull();

            }

            return modelo;
        }

        private static Marca getMarca(String name)
        {
            Marca marca = null;

            String fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                "<entity name=\"fib_marcaauto\">" +
                                    "<attribute name=\"fib_marcaautoid\" />" +
                                    "<attribute name=\"fib_name\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_name\" operator=\"like\" value=\"%" + name + "%\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";


            var service = CRMService.createService();
            var results = service.RetrieveMultiple(new FetchExpression(fetchXml));

            if (results.Entities.Any())
            {
                var entity = results.Entities[0];
                marca = new Marca();
                marca.Id = entity.AtributoColleccion("fib_marcaautoid").ToStringNull();
                marca.Name = entity.AtributoColleccion("fib_name").ToStringNull();

            }
            return marca;
        }

        private static Concesionaria getConcesionaria(String name)
        {
            Concesionaria concesionaria = null;

            String fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                "<entity name=\"fib_concesionaria\">" +
                                    "<attribute name=\"fib_concesionariaid\" />" +
                                    "<attribute name=\"fib_name\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_name\" operator=\"like\" value=\"%" + name + "%\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";


            var service = CRMService.createService();
            var results = service.RetrieveMultiple(new FetchExpression(fetchXml));

            if (results.Entities.Any())
            {
                var entity = results.Entities[0];
                concesionaria = new Concesionaria();
                concesionaria.Id = entity.AtributoColleccion("fib_concesionariaid").ToStringNull();
                concesionaria.Name = entity.AtributoColleccion("fib_name").ToStringNull();

            }
            return concesionaria;
        }

        private static TipoAutomovil getTipoAutomovil(String name)
        {
            TipoAutomovil tipoAutomovil = null;

            String fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                "<entity name=\"fib_tipoautomvil\">" +
                                    "<attribute name=\"fib_tipoautomvilid\" />" +
                                    "<attribute name=\"fib_name\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_name\" operator=\"like\" value=\"%" + name + "%\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";


            var service = CRMService.createService();
            var results = service.RetrieveMultiple(new FetchExpression(fetchXml));

            if (results.Entities.Any())
            {
                var entity = results.Entities[0];
                tipoAutomovil = new TipoAutomovil();
                tipoAutomovil.Id = entity.AtributoColleccion("fib_tipoautomvilid").ToStringNull();
                tipoAutomovil.Name = entity.AtributoColleccion("fib_name").ToStringNull();

            }

            return tipoAutomovil;
        }

        private static Disposicion getDisposicion(String numeroDisposicion)
        {
            Disposicion disp = null;

            String fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                "<entity name=\"fib_credito\">" +
                                    "<attribute name=\"fib_creditoid\" />" +
                                    "<attribute name=\"fib_lineadecreditoid\" />" +
                                    "<attribute name=\"fib_monedaid\" />" +
                                    "<attribute name=\"fib_monto\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_codigo\" operator=\"eq\" value=\"" + numeroDisposicion + "\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";


            var service = CRMService.createService();
            var results = service.RetrieveMultiple(new FetchExpression(fetchXml));

            if (results.Entities.Any())
            {
                var entity = results.Entities[0];
                disp = new Disposicion();
                disp.Id = entity.AtributoColleccion("fib_creditoid").ToStringNull();
                disp.LineaCreditoId = entity.AtributoColleccion("fib_lineadecreditoid",TipoAtributos.ENTITY_REFERENCE_ID).ToStringNull();
                disp.MonedaId = entity.AtributoColleccion("fib_monedaid",TipoAtributos.ENTITY_REFERENCE_ID).ToStringNull();
                disp.Monto = entity.AtributoColleccion("fib_monto",TipoAtributos.MONEY).ToDouble();

            }

            return disp;
        }

        
        private static Bitacora getBitacoraById(String bitacoraId)
        {
            Bitacora bitacora = null;

            String fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                "<entity name=\"fib_bitacoradisposicioneslinea\">" +
                                    "<attribute name=\"fib_credito\" />" +
                                    "<attribute name=\"fib_disposicion\" />" +
                                    "<attribute name=\"fib_name\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_bitacoradisposicioneslineaid\" operator=\"eq\" value=\"" + bitacoraId + "\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";


            var service = CRMService.createService();
            var results = service.RetrieveMultiple(new FetchExpression(fetchXml));

            if (results.Entities.Any())
            {
                var entity = results.Entities[0];
                bitacora = new Bitacora();
                bitacora.credito = entity.AtributoColleccion("fib_credito").ToStringNull();
                bitacora.disposicion = entity.AtributoColleccion("fib_disposicion").ToStringNull();
                bitacora.name = entity.AtributoColleccion("fib_name").ToStringNull();

            }

            return bitacora;
        }

        private static string getNumeroGarantiaById(String idGarantia)
        {
           String fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                "<entity name=\"fib_garantiadelineadecredito\">" +
                                    "<attribute name=\"fib_codigo\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_garantiadelineadecreditoid\" operator=\"eq\" value=\"" + idGarantia + "\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";


            var service = CRMService.createService();
            var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
            string numeroGarantia = null;
            if (results.Entities.Any())
            {
                var entity = results.Entities[0];
                numeroGarantia = entity.AtributoColleccion("fib_codigo").ToStringNull();

            }

            return numeroGarantia;
        }

        private static String getComision(String credito)
        {
            String comision = "0", cadFetch;
            cadFetch = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                   "      <entity name=\"fib_credito\">" +
                                   "          <attribute name=\"fib_creditoid\" />" +
                                   "          <attribute name=\"fib_monto\" />" +
                                   "          <attribute name=\"fib_iva\" />" +
                                   "		     <filter type=\"and\">" +
                                   "              <condition attribute=\"fib_codigo\" operator=\"eq\" value=\"" + credito + "\" />" +
                                   "          </filter>" +
                                   "      </entity>" +
                                   "  </fetch>";
            //String result = this.service.Fetch(cadFetch);
            //List<Hashtable> Trx = XmlUtil.XmlToMap(result);

            var service = CRMService.createService();
            var results = service.RetrieveMultiple(new FetchExpression(cadFetch));

            var creditoId = results.Entities[0].AtributoColleccion("fib_creditoid").ToStringNull();
            var monto = results.Entities[0].AtributoColleccion("fib_monto",TipoAtributos.MONEY).ToDecimal();
            var iva = results.Entities[0].AtributoColleccion("fib_iva").ToDecimal();
               
            cadFetch = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                  "      <entity name=\"fib_detallecomision\">" +
                                  "          <attribute name=\"fib_aplicaalcredito\" />" +
                                  "          <attribute name=\"fib_codigo\" />" +
                                  "          <attribute name=\"fib_porcentaje\" />" +
                                  "          <attribute name=\"fib_monto\" />" +
                                  "		     <filter type=\"and\">" +
                                  "              <condition attribute=\"fib_creditoid\" operator=\"eq\" value=\"" + creditoId + "\" />" +
                                  "              <condition attribute=\"fib_aplicaalcredito\" operator=\"eq\" value=\"1\" />" +
                                  "          </filter>" +
                                  "      </entity>" +
                                  "  </fetch>";
            //String result1 = this.service.Fetch(cadFetch);
            results = service.RetrieveMultiple(new FetchExpression(cadFetch));
            //List<Hashtable> com = XmlUtil.XmlToMap(result);
            if (results.Entities.Any())
            {
                comision = Convert.ToString((1 + (iva / 100)) * (Math.Round((Decimal)(monto) * (Decimal.Parse(results.Entities[0].AtributoColleccion("fib_porcentaje").ToString()) / 100), 2)));
            }
            return comision;
        }

        #endregion PRIVATE_METHODS

    }
}
