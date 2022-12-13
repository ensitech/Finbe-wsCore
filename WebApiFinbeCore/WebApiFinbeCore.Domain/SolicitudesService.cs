using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApiFinbeCore.Model;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System.Globalization;
using System.ServiceModel;
using Microsoft.Crm.Sdk.Messages;
using System.Net;
using System.Configuration;

using Microsoft.Xrm.Sdk.Discovery;

using Microsoft.Xrm.Sdk.Client;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Web.Hosting;
using System.IO;
using RestSharp;

namespace WebApiFinbeCore.Domain
{
    public class SolicitudesService
    {
        private static readonly int PERSONA_FISICA = 1;
        private static readonly int PERSONA_MORAL = 2;
        private static readonly int TELEFONO_FIJO = 1;
        private static readonly int TELEFONO_CELULAR = 2;
        private static readonly int TELEFONO_OTRO = 3;
        private static readonly int TIPO_SOLICITANTE = 1;
        private static readonly int TIPO_AVAL = 2;
        private static readonly string TIPO_BITACORA_PRIMER_ENVIO = "PRIMER ENVIO";
        private static readonly string TIPO_BITACORA_SEGUNDO_ENVIO = "SEGUNDO ENVIO";

        public static SolicitudResponse ProcesarSolicitud(Solicitud solicitud)
        {
            var json = JsonConvert.SerializeObject(solicitud);
            var response = new SolicitudResponse();
            var service = CRMService.createService();
            var idBitacora = CrearBitacora(solicitud.Folio, json, service, TIPO_BITACORA_PRIMER_ENVIO);
            var personas = new List<Persona>();
            Guid idSolicitud = Guid.Empty;
            Entity solicitudExistente = null;
            try
            {
                ValidarFolioDuplicado(solicitud.Folio, service, out solicitudExistente);
                //VALIDACION DE DATOS
                var datosValidos = ValidarDatos(solicitud, service, out string errorDatos);

                if (!datosValidos)
                {
                    ActualizarBitacora(idBitacora, errorDatos, service);
                }
                else
                {
                    //DATOS DE LA BITACORA SON VALIDOS
                    int tipoPersona = 0;
                    try
                    {
                        tipoPersona = ValidarRFC(solicitud.PersonaEvaluada.RFC, solicitud.PersonaEvaluada.TipoPersona);
                    }
                    catch (Exception e)
                    {
                        errorDatos += " Solicitante: " + e.Message;
                        tipoPersona = 0;
                    }
                    if (tipoPersona == PERSONA_FISICA)
                    {
                        string uuid = ExistePersonaFisica(solicitud.PersonaEvaluada.RFC, service);
                        if (uuid != null)
                        {
                            ActivarPersonaFisica(uuid, service);
                            var persona = new Persona()
                            {
                                Id = new Guid(uuid),
                                Solicitante = true,
                                Existe = true,
                                PersonaFisica = true
                            };
                            personas.Add(persona);
                            string numero = NumeroPersona(persona, service);
                            errorDatos += $"Persona Fisica Encontrada: [{numero}], ";
                        }
                        else
                        {
                            var idPersonaFisica = CrearPersonaFisica(solicitud.PersonaEvaluada, service);
                            var persona = new Persona()
                            {
                                Id = idPersonaFisica,
                                Solicitante = true,
                                PersonaFisica = true
                            };
                            personas.Add(persona);
                            string numero = NumeroPersona(persona, service);
                            errorDatos += $"Persona Fisica Creada: [{numero}], ";
                        }
                    }
                    else if (tipoPersona == PERSONA_MORAL)
                    {
                        string uuid = ExistePersonaMoral(solicitud.PersonaEvaluada.RFC, service);
                        if (uuid != null)
                        {
                            ActivarPersonaMoral(uuid, service);
                            var persona = new Persona()
                            {
                                Id = new Guid(uuid),
                                Solicitante = true,
                                Existe = true,
                                PersonaMoral = true
                            };
                            personas.Add(persona);
                            string numero = NumeroPersona(persona, service);
                            errorDatos += $"Persona Moral Encontrada: [{numero}], ";
                        }
                        else
                        {
                            var idPersonaMoral = CrearPersonaMoral(solicitud.PersonaEvaluada, service);
                            var persona = new Persona()
                            {
                                Id = idPersonaMoral,
                                Solicitante = true,
                                PersonaMoral = true
                            };
                            personas.Add(persona);
                            string numero = NumeroPersona(persona, service);
                            errorDatos += $"Persona Moral Creada: [{numero}], ";
                        }
                    }
                    if (personas.Any())
                    {
                        var solicitante = personas.First();
                        if (!solicitante.Existe)
                        {
                            //TELEFONOS COBRANZA (1) FIJO (2) 
                            if (!string.IsNullOrEmpty(solicitud.PersonaEvaluada.TelefonoCobranza1))
                            {
                                var telefonoContacto1 = CrearTelefonoContacto(solicitante, solicitud.PersonaEvaluada.TelefonoCobranza1,
                                    solicitud.PersonaEvaluada.ContactoCobranza, TELEFONO_FIJO, service);
                                solicitante.TelefonosContacto.Add(telefonoContacto1);
                            }
                            if (!string.IsNullOrEmpty(solicitud.PersonaEvaluada.TelefonoCobranza2))
                            {
                                var telefonoContacto2 = CrearTelefonoContacto(solicitante, solicitud.PersonaEvaluada.TelefonoCobranza2,
                                    solicitud.PersonaEvaluada.ContactoCobranza, TELEFONO_CELULAR, service);
                                solicitante.TelefonosContacto.Add(telefonoContacto2);
                            }

                        }
                        //CREAR SOLICITUD
                        idSolicitud = CrearSolicitud(solicitud, solicitante, service);
                        string numCredito = service.Retrieve("opportunity", idSolicitud, new ColumnSet(new string[1]
                        {
                          "fib_numcredito"
                        })).GetAttributeValue<string>("fib_numcredito");
                        errorDatos += $" Solicitud: [{numCredito}], ";
                        ActualizarBitacora(idBitacora, errorDatos, service);
                        AsignarSolicitudBitacora(idBitacora, idSolicitud, service);
                        if(!string.IsNullOrWhiteSpace(solicitud.Promotor))
                        {
                            try
                            {
                                AsignarOwnerSolicitud(solicitud.Promotor, idSolicitud, service);
                            }
                            catch (Exception e)
                            {
                                errorDatos += " Promotor: " + e.Message;
                            }
                        }
                        response.FolioSolicitud = numCredito;
                        response.Success = true;
                        if (solicitud.PersonaEvaluada.SolidariosObligados != null && solicitud.PersonaEvaluada.SolidariosObligados.Any())
                        {
                            string avalesError = "";
                            foreach (var solidario in solicitud.PersonaEvaluada.SolidariosObligados)
                            {
                                int tipoSolidario = 0;
                                try
                                {
                                    tipoSolidario = ValidarRFC(solidario.RFC, solidario.TipoPersona);
                                }
                                catch (Exception e)
                                {
                                    avalesError += " Aval: " + e.Message;
                                    tipoSolidario = 0;
                                }
                                if (tipoSolidario == PERSONA_FISICA)
                                {
                                    string uuid = ExistePersonaFisica(solidario.RFC, service);
                                    if (uuid != null)
                                    {
                                        ActivarPersonaFisica(uuid, service, TIPO_AVAL);
                                        var persona = new Persona()
                                        {
                                            Id = new Guid(uuid),
                                            Solicitante = false,
                                            Aval = true,
                                            Existe = true,
                                            PersonaFisica = true
                                        };
                                        personas.Add(persona);
                                        string numero = NumeroPersona(persona, service);
                                        avalesError += $"Persona Fisica Encontrada (Aval): [{numero}], ";
                                    }
                                    else
                                    {
                                        var idPersonaFisica = CrearPersonaFisica(solidario, service);
                                        var persona = new Persona()
                                        {
                                            Id = idPersonaFisica,
                                            Solicitante = false,
                                            Aval = true,
                                            PersonaFisica = true
                                        };
                                        personas.Add(persona);
                                        string numero = NumeroPersona(persona, service);
                                        avalesError += $"Persona Fisica Creada (Aval): [{numero}], ";
                                    }
                                }
                                else if (tipoSolidario == PERSONA_MORAL)
                                {
                                    string uuid = ExistePersonaMoral(solidario.RFC, service);
                                    if (uuid != null)
                                    {
                                        ActivarPersonaMoral(uuid, service, TIPO_AVAL);
                                        var persona = new Persona()
                                        {
                                            Id = new Guid(uuid),
                                            Solicitante = false,
                                            Aval = true,
                                            Existe = true,
                                            PersonaMoral = true
                                        };
                                        personas.Add(persona);
                                        string numero = NumeroPersona(persona, service);
                                        avalesError += $"Persona Moral Encontrada (Aval): [{numero}], ";
                                    }
                                    else
                                    {
                                        var idPersonaMoral = CrearPersonaMoral(solidario, service);
                                        var persona = new Persona()
                                        {
                                            Id = idPersonaMoral,
                                            Solicitante = false,
                                            Aval = true,
                                            PersonaMoral = true
                                        };
                                        personas.Add(persona);
                                        string numero = NumeroPersona(persona, service);
                                        avalesError += $"Persona Moral Creada (Aval): [{numero}], ";
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(avalesError))
                            {
                                errorDatos += "Avales: " + avalesError;
                            }
                            //CREAR RELACIONES SOLICITUD AVALES
                            var personasAvales = personas.Where(p => p.Aval).ToList();
                            if (personasAvales.Any())
                            {
                                foreach (var aval in personasAvales)
                                {
                                    var avalGuid = CrearAval(idSolicitud, aval, service);
                                    aval.RelacionAval = avalGuid;
                                }
                            }
                        }
                    }
                }
                ActualizarBitacora(idBitacora, errorDatos, service);
                response.FolioIMX = solicitud.Folio;
                response.Peticion = json;
                response.Respuesta = errorDatos;
            }
            catch (DuplicateRequestException ex)
            {
                response.FolioIMX = solicitud.Folio;
                response.FolioSolicitud = solicitudExistente != null && solicitudExistente.Attributes.Contains("fib_numcredito") ?
                    solicitudExistente.Attributes["fib_numcredito"].ToString() : string.Empty;
                response.Success = false;
                response.Peticion = json;
                response.Respuesta = ex.Message;
                ActualizarBitacora(idBitacora, ex.Message, service);
            }
            catch (Exception e)
            {
                response.FolioSolicitud = null;
                response.Success = false;
                string mensajes = "";
                if (personas.Any())
                {
                    var relacionAvales = personas.Where(p => p.RelacionAval != Guid.Empty).ToList();
                    foreach (var relacion in relacionAvales)
                    {
                        service.Delete("fib_aval", relacion.RelacionAval);
                    }
                }
                if (!idSolicitud.Equals(Guid.Empty))
                {
                    service.Delete("opportunity", idSolicitud);
                    mensajes += "Solicitud: Eliminada , ";
                }
                if (personas.Any())
                {
                    var personasNuevas = personas.Where(p => !p.Existe).ToList();
                    foreach (var persona in personasNuevas)
                    {
                        if (persona.TelefonosContacto.Any())
                        {
                            foreach (var telefonoContacto in persona.TelefonosContacto)
                            {
                                service.Delete("fib_telefonocontacto", telefonoContacto);
                                mensajes += "Telefono Contacto: Eliminado , ";
                            }
                        }
                        if (persona.PersonaFisica)
                        {
                            service.Delete("contact", persona.Id);
                            mensajes += "Persona Fisica: Eliminada, ";
                        }
                        else if (persona.PersonaMoral)
                        {
                            service.Delete("account", persona.Id);
                            mensajes += "Persona Moral: Eliminada, ";
                        }
                    }
                }
                if (mensajes.Length > 0)
                {
                    mensajes = "Se realiza el rollback: " + mensajes;
                }
                ActualizarBitacora(idBitacora, e.Message + " " + mensajes, service);
                response.Peticion = json;
                response.Respuesta = e.Message + " " + mensajes;
            }
            return response;
        }

        public static SolicitudResponse ProcesarActualizacionSolicitud(SolicitudActualizacion solicitud)
        {
            var json = JsonConvert.SerializeObject(solicitud);
            var response = new SolicitudResponse();
            var service = CRMService.createService();
            var personas = new List<Persona>();
            Guid idSolicitud = Guid.Empty;
            Guid idBitacora = CrearBitacora(solicitud.Folio, json, service, TIPO_BITACORA_SEGUNDO_ENVIO);

            try
            {
                var solicitudEntity = ExisteSolicitud(solicitud.Folio, service);

                idSolicitud = solicitudEntity.Id;

                var ultimaBitacora = ObtenerBitacora(solicitud.Folio, service);

                if(ultimaBitacora != null)
                {
                    var ultimoEnvio = ultimaBitacora.GetAttributeValue<string>("fib_tipoenvio");

                    if (!string.IsNullOrEmpty(ultimoEnvio) && ultimoEnvio == TIPO_BITACORA_SEGUNDO_ENVIO && ultimaBitacora.Attributes.Contains("fib_solicitudcredito"))
                    {
                        var solicitudReference = (EntityReference)ultimaBitacora.Attributes["fib_solicitudcredito"];
                        if(solicitudReference != null)
                        {
                            throw new DuplicateRequestException("El Folio-IMX: " + solicitud.Folio + " ya cuenta con una actualización, no es posible volverla a actualizar.");
                        }
                    }
                }

                //VALIDACION DE DATOS
                var datosValidos = true;
                var erroresDatos = "";

                if (solicitud.SolidariosObligados != null && solicitud.SolidariosObligados.Any())
                {
                    var solidariosValidos = ValidarSolidariosObligados(solicitud.SolidariosObligados, service, out string erroresSolidariosObligados);
                    if (!solidariosValidos)
                    {
                        erroresDatos += erroresSolidariosObligados;
                    }
                }

                if (solicitud.DetalleFinanciacion == null)
                {
                    erroresDatos += "El detalle de Financiacion es Requerido ";
                }
                else
                {
                    var valido = ValidarDatosFinanciacion(solicitud.DetalleFinanciacion, out string mensajesFinan);
                    if (!valido)
                    {
                        erroresDatos += "Errores en datos del Detalle de la Financiación: " + mensajesFinan;
                    }
                }

                if (!string.IsNullOrEmpty(erroresDatos))
                {
                    datosValidos = false;
                }

                if (!datosValidos)
                {
                    ActualizarBitacora(idBitacora, erroresDatos, service);
                }
                else
                {
                    //Actualizar Solicitud
                    ActualizarSolicitud(idSolicitud, solicitud.DetalleFinanciacion, service);
                    string numCredito = solicitudEntity.GetAttributeValue<string>("fib_numcredito");
                    erroresDatos += $" Solicitud: [{numCredito}] Actualizada, ";
                    ActualizarBitacora(idBitacora, erroresDatos, service);
                    AsignarSolicitudBitacora(idBitacora, idSolicitud, service);

                    response.FolioSolicitud = numCredito;
                    response.Success = true;

                    if (solicitud.SolidariosObligados != null && solicitud.SolidariosObligados.Any())
                    {
                        string avalesError = "";
                        foreach (var solidario in solicitud.SolidariosObligados)
                        {
                            int tipoSolidario = 0;
                            try
                            {
                                tipoSolidario = ValidarRFC(solidario.RFC, solidario.TipoPersona);
                            }
                            catch (Exception e)
                            {
                                avalesError += " Aval: " + e.Message;
                                tipoSolidario = 0;
                            }
                            if (tipoSolidario == PERSONA_FISICA)
                            {
                                string uuid = ExistePersonaFisica(solidario.RFC, service);
                                if (uuid != null)
                                {
                                    ActivarPersonaFisica(uuid, service, TIPO_AVAL);
                                    var persona = new Persona()
                                    {
                                        Id = new Guid(uuid),
                                        Solicitante = false,
                                        Aval = true,
                                        Existe = true,
                                        PersonaFisica = true
                                    };
                                    personas.Add(persona);
                                    string numero = NumeroPersona(persona, service);
                                    avalesError += $"Persona Fisica Encontrada (Aval): [{numero}], ";
                                }
                                else
                                {
                                    var idPersonaFisica = CrearPersonaFisica(solidario, service);
                                    var persona = new Persona()
                                    {
                                        Id = idPersonaFisica,
                                        Solicitante = false,
                                        Aval = true,
                                        PersonaFisica = true
                                    };
                                    personas.Add(persona);
                                    string numero = NumeroPersona(persona, service);
                                    avalesError += $"Persona Fisica Creada (Aval): [{numero}], ";
                                }
                            }
                            else if (tipoSolidario == PERSONA_MORAL)
                            {
                                string uuid = ExistePersonaMoral(solidario.RFC, service);
                                if (uuid != null)
                                {
                                    ActivarPersonaMoral(uuid, service, TIPO_AVAL);
                                    var persona = new Persona()
                                    {
                                        Id = new Guid(uuid),
                                        Solicitante = false,
                                        Aval = true,
                                        Existe = true,
                                        PersonaMoral = true
                                    };
                                    personas.Add(persona);
                                    string numero = NumeroPersona(persona, service);
                                    avalesError += $"Persona Moral Encontrada (Aval): [{numero}], ";
                                }
                                else
                                {
                                    var idPersonaMoral = CrearPersonaMoral(solidario, service);
                                    var persona = new Persona()
                                    {
                                        Id = idPersonaMoral,
                                        Solicitante = false,
                                        Aval = true,
                                        PersonaMoral = true
                                    };
                                    personas.Add(persona);
                                    string numero = NumeroPersona(persona, service);
                                    avalesError += $"Persona Moral Creada (Aval): [{numero}], ";
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(avalesError))
                        {
                            erroresDatos += "Avales: " + avalesError;
                        }
                        //CREAR RELACIONES SOLICITUD AVALES
                        var personasAvales = personas.Where(p => p.Aval).ToList();
                        if (personasAvales.Any())
                        {
                            foreach (var aval in personasAvales)
                            {
                                Guid avalGuid = Guid.Empty;
                                var avalExistente = ExisteAvalSolicitud(idSolicitud, aval, service);

                                if(avalExistente == null)
                                {
                                    avalGuid = CrearAval(idSolicitud, aval, service);
                                } else
                                {
                                    avalGuid = avalExistente.Id;
                                }

                                aval.RelacionAval = avalGuid;
                            }
                        }
                    }
                }

                ActualizarBitacora(idBitacora, erroresDatos, service);
                response.FolioIMX = solicitud.Folio;
                response.Peticion = json;
                response.Respuesta = erroresDatos;
            }
            catch (Exception ex)
            {
                response.FolioSolicitud = solicitud.Folio;
                response.Success = false;
                string mensajes = "";
                if (personas.Any())
                {
                    var relacionAvales = personas.Where(p => p.RelacionAval != Guid.Empty).ToList();
                    foreach (var relacion in relacionAvales)
                    {
                        service.Delete("fib_aval", relacion.RelacionAval);
                    }
                }
                if (personas.Any())
                {
                    var personasNuevas = personas.Where(p => !p.Existe).ToList();
                    foreach (var persona in personasNuevas)
                    {
                        if (persona.TelefonosContacto.Any())
                        {
                            foreach (var telefonoContacto in persona.TelefonosContacto)
                            {
                                service.Delete("fib_telefonocontacto", telefonoContacto);
                                mensajes += "Telefono Contacto: Eliminado , ";
                            }
                        }
                        if (persona.PersonaFisica)
                        {
                            service.Delete("contact", persona.Id);
                            mensajes += "Persona Fisica: Eliminada, ";
                        }
                        else if (persona.PersonaMoral)
                        {
                            service.Delete("account", persona.Id);
                            mensajes += "Persona Moral: Eliminada, ";
                        }
                    }
                }
                if (mensajes.Length > 0)
                {
                    mensajes = "Se realiza el rollback: " + mensajes;
                }
                ActualizarBitacora(idBitacora, ex.Message + " " + mensajes, service);
                response.Peticion = json;
                response.Respuesta = ex.Message + " " + mensajes;
            }

            return response;
        }

        public static SolicitudResponse ProcesarSolicitudIMX(Solicitud solicitud)
        {
            var response = new SolicitudResponse();
            response.Success = true;
            response.Respuesta = "Respuesta IMX";
            try
            {
                var url_base = ConfigurationManager.AppSettings["API_FACT"];
                var client = new RestClient(url_base);
                var request = new RestRequest("create_imx", Method.POST);
                request.AddJsonBody(solicitud);
                var clientResponse = client.Execute(request);
                response.Respuesta = clientResponse.Content.ToString();
            } catch(Exception e)
            {
                response.Success = false;
            }
            return response;
        }

        public static void GeneraBitacoraModelo(Solicitud solicitud, string errors)
        {
            var service = CRMService.createService();
            var json = JsonConvert.SerializeObject(solicitud);
            Entity bitacora = new Entity("fib_bitacorasolicitudescredito");
            bitacora.Attributes.Add("fib_name", solicitud.Folio);
            bitacora.Attributes.Add("fib_datosentradamultiline", json);
            bitacora.Attributes.Add("fib_resultados", errors);
            var createResponse = service.Create(bitacora);
        }

        private static void ValidarFolioDuplicado(string folio, IOrganizationService service, out Entity solicitudExistente)
        {
            solicitudExistente = null;
            var bitacora = ObtenerBitacora(folio, service);

            if (bitacora != null && bitacora.Attributes.Contains("fib_solicitudcredito"))
            {
                var solicitudReference = (EntityReference)bitacora.Attributes["fib_solicitudcredito"];

                solicitudExistente = service.Retrieve(solicitudReference.LogicalName, solicitudReference.Id, new ColumnSet(new string[] { "fib_numcredito" }));

                if (solicitudExistente != null && solicitudExistente.Attributes.Contains("fib_numcredito"))
                    throw new DuplicateRequestException("El Folio-IMX: " + folio + " está relacionado a una solicitud con Folio: " + solicitudExistente.Attributes["fib_numcredito"] + ". No es posible duplicar la solicitud");
            }
        }

        private static Entity ObtenerBitacora(string folio, IOrganizationService service)
        {
            String fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                "<entity name=\"fib_bitacorasolicitudescredito\">" +
                                    "<attribute name=\"fib_solicitudcredito\" />" +
                                    "<attribute name=\"fib_tipoenvio\" />" +
                                    "<attribute name=\"createdon\" />" +
                                    "<order attribute=\"createdon\" descending=\"true\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_name\" operator=\"eq\" value=\"" + folio + "\" />" +
                                        "<condition attribute=\"fib_solicitudcredito\" operator=\"not-null\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";
            var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return results.Entities.FirstOrDefault();
        }

        private static Entity ObtenerUltimaBitacora(string folio, IOrganizationService service)
        {
            string fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                "<entity name=\"fib_bitacorasolicitudescredito\">" +
                                    "<attribute name=\"createdon\" />" +
                                    "<order attribute=\"createdon\" descending=\"true\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_name\" operator=\"eq\" value=\"" + folio + "\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";
            var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
            return results.Entities.FirstOrDefault();
        }

        /*private static List<ValorLista> Lista(string nombreLista)
        {
            var valores = new List<ValorLista>();

            try
            {
                var service = CRMService.createService();

                // Use the RetrieveOptionSetRequest message to retrieve  
                // a global option set by it's name.
                RetrieveOptionSetRequest retrieveOptionSetRequest =
                    new RetrieveOptionSetRequest
                    {
                        Name = nombreLista

                    };

                // Execute the request.
                RetrieveOptionSetResponse retrieveOptionSetResponse =
                    (RetrieveOptionSetResponse)service.Execute(
                    retrieveOptionSetRequest);

                // Access the retrieved OptionSetMetadata.
                OptionSetMetadata retrievedOptionSetMetadata =
                    (OptionSetMetadata)retrieveOptionSetResponse.OptionSetMetadata;

                // Get the current options list for the retrieved attribute.
                OptionMetadata[] optionList =
                    retrievedOptionSetMetadata.Options.ToArray();
                var opciones = optionList.Select(o => new ValorLista() { Nombre = o.Label.UserLocalizedLabel.Label, Valor = o?.Value }).ToList();
                valores.AddRange(opciones);
            }
            catch (ConectorCRMException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception("FB-DL 2: Error al recuperar información de la lista "+nombreLista+" "+ex.Message);
            }
            return valores;
        }*/

        #region Bitacora
        private static Guid CrearBitacora(string folio, string datosEntrada, IOrganizationService service, string tipoEnvio = "")
        {
            try
            {
                Entity bitacora = new Entity("fib_bitacorasolicitudescredito");
                bitacora.Attributes.Add("fib_name", folio);
                bitacora.Attributes.Add("fib_datosentradamultiline", datosEntrada);
                bitacora.Attributes.Add("fib_tipoenvio", tipoEnvio);
                var createResponse = service.Create(bitacora);
                return new Guid(createResponse.ToString());
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
            catch (ConectorCRMException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception("FB-DL 2: Error al crear la bitacora", ex);
            }
        }

        private static Guid CrearBitacoraDocumento(string folio, string nombreDocumento, bool esUltimoDocumento, IOrganizationService service)
        {
            try
            {
                Entity bitacora = new Entity("fib_bitacoradocumentosolicitud");
                bitacora.Attributes.Add("fib_name", folio);
                bitacora.Attributes.Add("fib_nombredocumento", nombreDocumento);
                bitacora.Attributes.Add("fib_ultimoenvio", esUltimoDocumento);
                var createResponse = service.Create(bitacora);
                return new Guid(createResponse.ToString());
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw ex;
            }
            catch (ConectorCRMException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception("FB-DL 2: Error al crear la bitacora", ex);
            }
        }

        private static Entity ActualizarBitacora(Guid idBitacora, string Resultados, IOrganizationService service)
        {
            try
            {
                Entity bitacora = new Entity("fib_bitacorasolicitudescredito");
                bitacora.Id = idBitacora;
                bitacora.Attributes.Add("fib_resultados", Resultados);
                service.Update(bitacora);
                return bitacora;
            }
            catch (ConectorCRMException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception("FB-DL 2: Error al actualizar los resultados de la bitacora", ex);
            }
        }

        private static Entity ActualizarBitacoraDocumento(Guid idBitacoraDocumento, string Resultados, IOrganizationService service)
        {
            try
            {
                Entity bitacora = new Entity("fib_bitacoradocumentosolicitud");
                bitacora.Id = idBitacoraDocumento;
                bitacora.Attributes.Add("fib_resultados", Resultados);
                service.Update(bitacora);
                return bitacora;
            }
            catch (ConectorCRMException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception("FB-DL 2: Error al actualizar los resultados de la bitacora", ex);
            }
        }

        public static Entity AsignarSolicitudBitacora(Guid idBitacora, Guid solicitudUuid, IOrganizationService service)
        {
            try
            {
                Entity bitacora = new Entity("fib_bitacorasolicitudescredito");
                bitacora.Id = idBitacora;
                bitacora.Attributes.Add("fib_solicitudcredito", new EntityReference("opportunity", solicitudUuid));
                service.Update(bitacora);
                return bitacora;
            }
            catch (ConectorCRMException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception("FB-DL 2: Error al asignar la solicitud en la bitacora", ex);
            }
        }

        public static Entity AsignarReferenciasBitacoraDocumento(Guid idBitacoraDocumento, Guid solicitudUuid, Guid bitacoraPrincipalUuid, IOrganizationService service)
        {
            try
            {
                Entity bitacora = new Entity("fib_bitacoradocumentosolicitud");
                bitacora.Id = idBitacoraDocumento;
                bitacora.Attributes.Add("fib_solicitudcreditoid", new EntityReference("opportunity", solicitudUuid));
                bitacora.Attributes.Add("fib_bitacorasolicitudcredito", new EntityReference("fib_bitacorasolicitudescredito", bitacoraPrincipalUuid));
                service.Update(bitacora);
                return bitacora;
            }
            catch (ConectorCRMException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception("FB-DL 2: Error al asignar la solicitud en la bitacora", ex);
            }
        }

        public static Entity AsignarBitacoraSolicitudBitacoraDocumento(Guid idBitacoraDocumento, Guid bitacoraPrincipalUuid, IOrganizationService service)
        {
            try
            {
                Entity bitacora = new Entity("fib_bitacoradocumentosolicitud");
                bitacora.Id = idBitacoraDocumento;
                bitacora.Attributes.Add("fib_bitacorasolicitudcredito", new EntityReference("fib_bitacorasolicitudescredito", bitacoraPrincipalUuid));
                service.Update(bitacora);
                return bitacora;
            }
            catch (ConectorCRMException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception("FB-DL 2: Error al asignar la solicitud en la bitacora", ex);
            }
        }

        public static Entity AsignarSolicitudBitacoraDocumento(Guid idBitacoraDocumento, Guid solicitudUuid, IOrganizationService service)
        {
            try
            {
                Entity bitacora = new Entity("fib_bitacoradocumentosolicitud");
                bitacora.Id = idBitacoraDocumento;
                bitacora.Attributes.Add("fib_solicitudcreditoid", new EntityReference("opportunity", solicitudUuid));
                service.Update(bitacora);
                return bitacora;
            }
            catch (ConectorCRMException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception("FB-DL 2: Error al asignar la solicitud en la bitacora", ex);
            }
        }
        #endregion

        #region PersonaFisica
        public static string ExistePersonaFisica(string RFC, IOrganizationService service)
        {
            string id = null;

            String fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                "<entity name=\"contact\">" +
                                    "<attribute name=\"contactid\" />" +
                                    "<attribute name=\"statecode\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_rfc\" operator=\"eq\" value=\"" + RFC + "\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";

            try
            {
                var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (results.Entities.Any())
                {
                    id = results.Entities[0].AtributoColleccion("contactid", TipoAtributos.STRING).ToString();
                }
            }
            catch
            {
                throw new ConectorCRMException("FB-CRM 4: Error al recuperar información del cliente");
            }

            return id;
        }

        public static void ActivarPersonaFisica(string idPersonaFisica, IOrganizationService service, int tipo = 1)
        {
            try
            {
                Entity contact = new Entity("contact")
                {
                    Id = new Guid(idPersonaFisica)
                };
                contact.Attributes.Add("statecode", 0);
                if (tipo == TIPO_SOLICITANTE)
                {
                    contact.Attributes.Add("fib_solicitante", true);
                }
                else if (tipo == TIPO_AVAL)
                {
                    contact.Attributes.Add("fib_aval", true);
                }
                service.Update(contact);
            }
            catch
            {
                throw new ConectorCRMException("FB-CRM 4: Error al cambiar de estatus el cliente");

            }
        }
        /// <summary>
        /// METODO QUE FUNCIONA test
        /// </summary>
        /// <param name="personaFisica"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        public static Guid CrearPersonaFisica1(PersonaEvaluada personaFisica, IOrganizationService service)
        {

            try
            {
                Entity contact = new Entity("contact");
                //con actividad empresarial
                contact.Attributes.Add("customertypecode", new OptionSetValue(200000));

                contact.Attributes.Add("lastname", "Chan");
                contact.Attributes.Add("fib_apellidomaterno", "Salas");
                contact.Attributes.Add("firstname", "Celina");
                contact.Attributes.Add("gendercode", new OptionSetValue(2));

                contact.Attributes.Add("birthdate", new DateTime(1988, 8, 17));

                contact.Attributes.Add("telephone2", "9997475948");
                contact.Attributes.Add("mobilephone", "9997475948");
                contact.Attributes.Add("emailaddress1", "wcaamal@gmail.com");

                contact.Attributes.Add("fib_rfc", "CASC880817549");

                contact.Attributes.Add("address1_line1", "Conocido");


                contact.Attributes.Add("address1_postalcode", "97302");


                contact.Attributes.Add("fib_coloniaid", new EntityReference("fib_colonia", new Guid("4B2582F0-AAB5-E711-8B9B-005056855416")));
                contact.Attributes.Add("fib_delegmposid", new EntityReference("fib_municipio", new Guid("92D5F3DD-3947-DF11-8D53-005056977B10")));
                contact.Attributes.Add("new_estadoid", new EntityReference("new_estado", new Guid("66F63CB4-3947-DF11-8D53-005056977B10")));
                contact.Attributes.Add("fib_ciudadid", new EntityReference("fib_ciudad", new Guid("272A9131-3A47-DF11-8D53-005056977B10")));
                contact.Attributes.Add("new_paisid", new EntityReference("new_pais", new Guid("930FBAA0-3947-DF11-8D53-005056977B10")));

                var createResponse = service.Create(contact);

                return new Guid(createResponse.ToString());
            }
            catch (ConectorCRMException ex)
            {
                throw ex;
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception("FB-DL 2: Error al crear la persona fisica - " + personaFisica.RFC, ex);
            }
        }

        public static Guid CrearPersonaFisica(PersonaEvaluada personaFisica, IOrganizationService service)
        {

            try
            {
                Entity contact = new Entity("contact");
                //con actividad empresarial
                contact.Attributes.Add("customertypecode", new OptionSetValue(200000));
                contact.Attributes.Add("lastname", personaFisica.ApellidoPaterno);
                contact.Attributes.Add("fib_solicitante", true);
                contact.Attributes.Add("gendercode", new OptionSetValue(personaFisica.Genero));
                string[] nombre = personaFisica.Nombres.Split(new char[] { ' ' }, 2);
                contact.Attributes.Add("firstname", nombre[0].Trim());
                contact.Attributes.Add("middlename", nombre.Length > 1 ? nombre[1].Trim() : "");

                if (string.IsNullOrEmpty(personaFisica.ApellidoMaterno))
                {
                    contact.Attributes.Add("fib_sinapellidomaterno", true);
                }
                else
                {
                    contact.Attributes.Add("fib_apellidomaterno", personaFisica.ApellidoMaterno);
                }

                contact.Attributes.Add("fib_rfc", personaFisica.RFC);
                var fecha = DateTime.ParseExact(personaFisica.FechaNacimiento, "yyyy-MM-dd", new CultureInfo("es-MX"));
                contact.Attributes.Add("birthdate", fecha);

                if (personaFisica.EstadoCivil != null)
                {
                    contact.Attributes.Add("familystatuscode", new OptionSetValue(personaFisica.EstadoCivil.Value));
                }
                if (personaFisica.DocumentoIdentificacion != null)
                {
                    contact.Attributes.Add("fib_tipoidentificacionpersona", new OptionSetValue(personaFisica.DocumentoIdentificacion.Value));
                }
                if (!string.IsNullOrEmpty(personaFisica.NumeroIdentificacion))
                {
                    contact.Attributes.Add("fib_numidentificacionpersona", personaFisica.NumeroIdentificacion);
                }

                contact.Attributes.Add("fullname", personaFisica.Nombres + " " + personaFisica.ApellidoPaterno + (string.IsNullOrEmpty(personaFisica.ApellidoMaterno) ? "" : " " + personaFisica.ApellidoMaterno));
                contact.Attributes.Add("emailaddress1", personaFisica.EmailPFE);
                contact.Attributes.Add("mobilephone", personaFisica.TelefonoMovil);

                if (!string.IsNullOrEmpty(personaFisica.TelefonoOficina))
                {
                    contact.Attributes.Add("fib_domiciliotelefononegocio", personaFisica.TelefonoOficina);
                }
                if (!string.IsNullOrEmpty(personaFisica.EmailFacturacion))
                {
                    contact.Attributes.Add("fib_correoelectronicoadicional", personaFisica.EmailFacturacion);
                }
                if (!string.IsNullOrEmpty(personaFisica.LugarNacimiento))
                {
                    contact.Attributes.Add("fib_estadonacimiento", new EntityReference("new_estado", personaFisica.EstadoNacimientoId));
                    contact.Attributes.Add("fib_paisdenacimientoid", new EntityReference("new_pais", personaFisica.PaisNacimientoId));
                }
                //
                //fib_nacionalidad
                if (personaFisica.Nacionalidad != null)
                {
                    contact.Attributes.Add("fib_nacionalidad", new OptionSetValue(personaFisica.Nacionalidad.Value));
                }
                //fib_profesion
                if (personaFisica.Profesion != null)
                {
                    contact.Attributes.Add("fib_profesion", new OptionSetValue(personaFisica.Profesion.Value));
                }
                if (!string.IsNullOrEmpty(personaFisica.GiroOActividad))
                {
                    var actividadID = ExisteActividadOGiro(personaFisica.GiroOActividad, service);
                    if (actividadID != Guid.Empty)
                    {
                        contact.Attributes.Add("fib_actividadeconomicapldid", new EntityReference("fib_actividadeconomicapld", actividadID));
                    }
                    
                }
                //domicilio
                LlenarDatosDomicilioPersonaFisica(contact, personaFisica.DomicilioFacturacion);

                ///probar domicilio operativo
                if (personaFisica.DomicilioOperativo != null)
                {
                    LlenarDatosDomicilioAdicionalPersonaFisica(contact, personaFisica.DomicilioOperativo);
                }

                if (personaFisica.Impuestos > 0)
                {
                    contact.Attributes.Add("fib_impuestosaplicables", new OptionSetValue(personaFisica.Impuestos));
                }

                if (!string.IsNullOrEmpty(personaFisica.FormaPago))
                {
                    var formaPagoGuid = FindEntityId("fib_formadepago", personaFisica.FormaPago, "fib_codigo", service);

                    if (formaPagoGuid != null && formaPagoGuid != Guid.Empty)
                    {
                        contact.Attributes.Add("fib_formadepagoid", new EntityReference("fib_formadepago", formaPagoGuid));
                    }
                }

                if (!string.IsNullOrEmpty(personaFisica.UsoComprobante))
                {
                    var usoComprobanteGuid = FindEntityId("fib_usocomprobante", personaFisica.UsoComprobante, "fib_codigo", service);

                    if (usoComprobanteGuid != null && usoComprobanteGuid != Guid.Empty)
                    {
                        contact.Attributes.Add("fib_usocomprobanteid", new EntityReference("fib_usocomprobante", usoComprobanteGuid));
                    }
                }

                if (!string.IsNullOrEmpty(personaFisica.RegimenFiscal))
                {
                    var regimenFiscalGuid = FindEntityId("fib_regimenfiscal", personaFisica.RegimenFiscal, "fib_codigo", service);

                    if (regimenFiscalGuid != null && regimenFiscalGuid != Guid.Empty)
                    {
                        contact.Attributes.Add("fib_regimenfiscalid", new EntityReference("fib_regimenfiscal", regimenFiscalGuid));
                    }
                }

                if (personaFisica.DomicilioFacturacion != null && !string.IsNullOrEmpty(personaFisica.CodigoPostalV4))
                    contact.Attributes.Add("fib_codigopostalcfdi", personaFisica.CodigoPostalV4);

                var createResponse = service.Create(contact);

                return new Guid(createResponse.ToString());
            }
            catch (ConectorCRMException ex)
            {
                throw ex;
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception("FB-DL 2: Error al crear la persona fisica - " + personaFisica.RFC, ex);
            }
        }

        public static Guid CrearPersonaFisica(SolidarioObligado personaFisica, IOrganizationService service)
        {

            try
            {
                Entity contact = new Entity("contact");
                //con actividad empresarial
                contact.Attributes.Add("customertypecode", new OptionSetValue(200000));
                contact.Attributes.Add("lastname", personaFisica.ApellidoPaterno);
                contact.Attributes.Add("fib_aval", true);
                contact.Attributes.Add("fib_solicitante", false);
                contact.Attributes.Add("gendercode", new OptionSetValue(personaFisica.Genero));
                string[] nombre = personaFisica.Nombres.Split(new char[] { ' ' }, 2);
                contact.Attributes.Add("firstname", nombre[0].Trim());
                contact.Attributes.Add("middlename", nombre.Length > 1 ? nombre[1].Trim() : "");

                if (string.IsNullOrEmpty(personaFisica.ApellidoMaterno))
                {
                    contact.Attributes.Add("fib_sinapellidomaterno", true);
                }
                else
                {
                    contact.Attributes.Add("fib_apellidomaterno", personaFisica.ApellidoMaterno);
                }

                contact.Attributes.Add("fib_rfc", personaFisica.RFC);
                var fecha = DateTime.ParseExact(personaFisica.FechaNacimiento, "yyyy-MM-dd", new CultureInfo("es-MX"));
                contact.Attributes.Add("birthdate", fecha);

                if (personaFisica.EstadoCivil != null)
                {
                    contact.Attributes.Add("familystatuscode", new OptionSetValue(personaFisica.EstadoCivil.Value));
                }
                if (personaFisica.DocumentoIdentificacion != null)
                {
                    contact.Attributes.Add("fib_tipoidentificacionpersona", new OptionSetValue(personaFisica.DocumentoIdentificacion.Value));
                }
                if (!string.IsNullOrEmpty(personaFisica.NumeroIdentificacion))
                {
                    contact.Attributes.Add("fib_numidentificacionpersona", personaFisica.NumeroIdentificacion);
                }

                contact.Attributes.Add("fullname", personaFisica.Nombres + " " + personaFisica.ApellidoPaterno + (string.IsNullOrEmpty(personaFisica.ApellidoMaterno) ? "" : " " + personaFisica.ApellidoMaterno));
                contact.Attributes.Add("emailaddress1", personaFisica.EmailPFE);
                contact.Attributes.Add("mobilephone", personaFisica.TelefonoMovil);

                if (!string.IsNullOrEmpty(personaFisica.TelefonoOficina))
                {
                    contact.Attributes.Add("fib_domiciliotelefononegocio", personaFisica.TelefonoOficina);
                }
                if (!string.IsNullOrEmpty(personaFisica.EmailFacturacion))
                {
                    contact.Attributes.Add("fib_correoelectronicoadicional", personaFisica.EmailFacturacion);
                }
                if (!string.IsNullOrEmpty(personaFisica.LugarNacimiento))
                {
                    contact.Attributes.Add("fib_estadonacimiento", new EntityReference("new_estado", personaFisica.EstadoNacimientoId));
                    contact.Attributes.Add("fib_paisdenacimientoid", new EntityReference("new_pais", personaFisica.PaisNacimientoId));
                }
                //
                //fib_nacionalidad
                if (personaFisica.Nacionalidad != null)
                {
                    contact.Attributes.Add("fib_nacionalidad", new OptionSetValue(personaFisica.Nacionalidad.Value));
                }
                //fib_profesion
                if (personaFisica.Profesion != null)
                {
                    contact.Attributes.Add("fib_profesion", new OptionSetValue(personaFisica.Profesion.Value));
                }
                if (!string.IsNullOrEmpty(personaFisica.GiroOActividad))
                {
                    var actividadID = ExisteActividadOGiro(personaFisica.GiroOActividad, service);
                    if (actividadID != Guid.Empty)
                        contact.Attributes.Add("fib_actividadeconomicapldid", new EntityReference("fib_actividadeconomicapld", actividadID));
                }
                //domicilio
                LlenarDatosDomicilioPersonaFisica(contact, personaFisica.DomicilioFacturacion);

                ///probar domicilio operativo
                if (personaFisica.DomicilioOperativo != null)
                {
                    LlenarDatosDomicilioAdicionalPersonaFisica(contact, personaFisica.DomicilioOperativo);
                }

                if (personaFisica.Impuestos > 0)
                {
                    contact.Attributes.Add("fib_impuestosaplicables", new OptionSetValue(personaFisica.Impuestos));
                }

                if (!string.IsNullOrEmpty(personaFisica.FormaPago))
                {
                    var formaPagoGuid = FindEntityId("fib_formadepago", personaFisica.FormaPago, "fib_codigo", service);

                    if (formaPagoGuid != null && formaPagoGuid != Guid.Empty)
                    {
                        contact.Attributes.Add("fib_formadepagoid", new EntityReference("fib_formadepago", formaPagoGuid));
                    }
                }

                if (!string.IsNullOrEmpty(personaFisica.UsoComprobante))
                {
                    var usoComprobanteGuid = FindEntityId("fib_usocomprobante", personaFisica.UsoComprobante, "fib_codigo", service);

                    if (usoComprobanteGuid != null && usoComprobanteGuid != Guid.Empty)
                    {
                        contact.Attributes.Add("fib_usocomprobanteid", new EntityReference("fib_usocomprobante", usoComprobanteGuid));
                    }
                }

                if (!string.IsNullOrEmpty(personaFisica.RegimenFiscal))
                {
                    var regimenFiscalGuid = FindEntityId("fib_regimenfiscal", personaFisica.RegimenFiscal, "fib_codigo", service);

                    if (regimenFiscalGuid != null && regimenFiscalGuid != Guid.Empty)
                    {
                        contact.Attributes.Add("fib_regimenfiscalid", new EntityReference("fib_regimenfiscal", regimenFiscalGuid));
                    }
                }

                if (personaFisica.DomicilioFacturacion != null && !string.IsNullOrEmpty(personaFisica.CodigoPostalV4))
                    contact.Attributes.Add("fib_codigopostalcfdi", personaFisica.CodigoPostalV4);

                var createResponse = service.Create(contact);

                return new Guid(createResponse.ToString());
            }
            catch (ConectorCRMException ex)
            {
                throw ex;
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception("FB-DL 2: Error al crear la persona fisica (aval) - " + personaFisica.RFC, ex);
            }
        }

        private static void LlenarDatosDomicilioPersonaFisica(Entity personaFisica, Domicilio domicilio)
        {
            personaFisica.Attributes.Add("address1_line1",
                    string.Format("{0} {1}",
                    domicilio.Calle,
                    (!string.IsNullOrEmpty(domicilio.NumeroInterior) ? "#Int:" + domicilio.NumeroInterior + " " : "")
                    +
                   "#Ext:" + domicilio.NumeroExterior
                    )
                    );
            personaFisica.Attributes.Add("address1_postalcode", domicilio.CodigoPostal);
            personaFisica.Attributes.Add("fib_coloniaid", new EntityReference("fib_colonia", domicilio.DomicilioCodigoPostal.Colonias.First().ColoniaId));
            personaFisica.Attributes.Add("fib_delegmposid", new EntityReference("fib_municipio", domicilio.DomicilioCodigoPostal.MunicipioId));
            personaFisica.Attributes.Add("new_estadoid", new EntityReference("new_estado", domicilio.DomicilioCodigoPostal.EstadoId));
            personaFisica.Attributes.Add("fib_ciudadid", new EntityReference("fib_ciudad", domicilio.DomicilioCodigoPostal.CiudadId));
            personaFisica.Attributes.Add("new_paisid", new EntityReference("new_pais", domicilio.DomicilioCodigoPostal.PaisId));
        }

        private static void LlenarDatosDomicilioAdicionalPersonaFisica(Entity personaFisica, Domicilio domicilio)
        {
            if (domicilio.DomicilioCodigoPostal != null)
            {
                personaFisica.Attributes.Add("fib_domicilioadicional1", true);
                personaFisica.Attributes.Add("fib_address3_line1",
                    string.Format("{0} {1}",
                    domicilio.Calle,
                    (!string.IsNullOrEmpty(domicilio.NumeroInterior) ? "#Int:" + domicilio.NumeroInterior + " " : "")
                    +
                   "#Ext:" + domicilio.NumeroExterior
                    )
                    );

                personaFisica.Attributes.Add("fib_address3_postalcode", domicilio.CodigoPostal);

                personaFisica.Attributes.Add("fib_coloniaid3", new EntityReference("fib_colonia", domicilio.DomicilioCodigoPostal.Colonias.First().ColoniaId));
                personaFisica.Attributes.Add("fib_municipioid3", new EntityReference("fib_municipio", domicilio.DomicilioCodigoPostal.MunicipioId));
                personaFisica.Attributes.Add("fib_estadoid3", new EntityReference("new_estado", domicilio.DomicilioCodigoPostal.EstadoId));
                personaFisica.Attributes.Add("fib_ciudadid3", new EntityReference("fib_ciudad", domicilio.DomicilioCodigoPostal.CiudadId));
                personaFisica.Attributes.Add("fib_paisid3", new EntityReference("new_pais", domicilio.DomicilioCodigoPostal.PaisId));
            }
            else
            {
                personaFisica.Attributes.Add("fib_domicilioadicional1", false);
            }
        }

        public static Guid CrearTelefonoContacto(Persona persona, string telefono, string descripcion, int tipoTelefono, IOrganizationService service, bool principal = false)
        {

            try
            {
                Entity telefonoContacto = new Entity("fib_telefonocontacto");
                //con actividad empresarial
                telefonoContacto.Attributes.Add("fib_name", descripcion);
                telefonoContacto.Attributes.Add("fib_principal", principal);
                telefonoContacto.Attributes.Add("fib_tipotelefono", new OptionSetValue(tipoTelefono));
                telefonoContacto.Attributes.Add("fib_telefono", telefono);
                telefonoContacto.Attributes.Add("fib_propiedad", new OptionSetValue(3));
                if (persona.PersonaFisica)
                {
                    telefonoContacto.Attributes.Add("fib_personafisicaid", new EntityReference("contact", persona.Id));
                }
                else if (persona.PersonaMoral)
                {
                    telefonoContacto.Attributes.Add("fib_personamoralid", new EntityReference("account", persona.Id));
                }
                var createResponse = service.Create(telefonoContacto);

                return new Guid(createResponse.ToString());
            }
            catch (ConectorCRMException ex)
            {
                throw ex;
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception("FB-DL 2: Error al asignar el telefono de contacto", ex);
            }
        }

        #endregion

        #region Persona Moral

        public static string ExistePersonaMoral(string RFC, IOrganizationService service)
        {
            string id = null;

            String fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                "<entity name=\"account\">" +
                                    "<attribute name=\"accountid\" />" +
                                    "<attribute name=\"statecode\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_rfc\" operator=\"eq\" value=\"" + RFC + "\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";

            try
            {
                var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (results.Entities.Any())
                {
                    id = results.Entities[0].AtributoColleccion("accountid", TipoAtributos.STRING).ToString();
                }
            }
            catch
            {
                throw new ConectorCRMException("FB-CRM 4: Error al recuperar información del cliente");
            }

            return id;
        }

        public static void ActivarPersonaMoral(string idPersonaMoral, IOrganizationService service, int tipo = 1)
        {
            try
            {
                Entity contact = new Entity("account")
                {
                    Id = new Guid(idPersonaMoral)
                };
                contact.Attributes.Add("statecode", 0);
                if (tipo == TIPO_SOLICITANTE)
                {
                    contact.Attributes.Add("fib_solicitante", true);
                }
                else if (tipo == TIPO_AVAL)
                {
                    contact.Attributes.Add("fib_aval", true);
                }
                service.Update(contact);
            }
            catch
            {
                throw new ConectorCRMException("FB-CRM 4: Error al cambiar de estatus el cliente");

            }
        }

        public static Guid CrearPersonaMoral(PersonaEvaluada personaMoral, IOrganizationService service)
        {

            try
            {
                Entity account = new Entity("account");

                account.Attributes.Add("name", personaMoral.RazonSocial);
                if (!string.IsNullOrEmpty(personaMoral.Nombre))
                {
                    account.Attributes.Add("fib_nombrecomercial", personaMoral.Nombre);
                }
                account.Attributes.Add("fib_solicitante", true);
                account.Attributes.Add("fib_aval", false);
                account.Attributes.Add("fib_accionista", false);
                account.Attributes.Add("fib_rfc", personaMoral.RFC);

                account.Attributes.Add("fib_email", personaMoral.EmailPFE);
                account.Attributes.Add("fib_mobilephone", personaMoral.TelefonoMovil);
                account.Attributes.Add("telephone1", personaMoral.TelefonoOficina);
                account.Attributes.Add("telephone3", personaMoral.TelefonoOficina);

                account.Attributes.Add("emailaddress1", personaMoral.EmailFacturacion);

                string[] nombre = personaMoral.ContactoCobranza.Split(new char[] { ' ' }, 4);
                account.Attributes.Add("address1_primarycontactname", nombre[0]);
                if (nombre.Length > 3)
                {
                    account.Attributes.Add("fib_segundonombre", nombre[1].Trim());
                    account.Attributes.Add("fib_apellidopaterno", nombre[2].Trim());
                    account.Attributes.Add("fib_apellidomaterno", nombre[3].Trim());
                }
                else if (nombre.Length > 2)
                {
                    account.Attributes.Add("fib_apellidopaterno", nombre[1].Trim());
                    account.Attributes.Add("fib_apellidomaterno", nombre[2].Trim());
                }
                //account.Attributes.Add("fib_nombreapoderadolegal", "Prueba");
                //account.Attributes.Add("fib_sexoapoderadolegal", new OptionSetValue(1));
                //account.Attributes.Add("fib_tipodepersonamoral", new OptionSetValue(1));
                if (!string.IsNullOrEmpty(personaMoral.GiroOActividad))
                {
                    var actividadID = ExisteActividadOGiro(personaMoral.GiroOActividad, service);
                    if (actividadID != Guid.Empty)
                    {
                        account.Attributes.Add("fib_actividadeconomicapldid", new EntityReference("fib_actividadeconomicapld", actividadID));
                    }
                }
                //domicilio
                LlenarDatosDomicilioPersonaMoral(account, personaMoral.DomicilioFacturacion);

                ///probar domicilio operativo
                if (personaMoral.DomicilioOperativo != null)
                {
                    LlenarDatosDomicilioAdicionalPersonaMoral(account, personaMoral.DomicilioOperativo);
                }

                if (!string.IsNullOrEmpty(personaMoral.RazonSocialV4))
                {
                    account.Attributes.Add("fib_razonsocialcfdi", personaMoral.RazonSocialV4);
                }
                
                if (personaMoral.Impuestos > 0)
                {
                    account.Attributes.Add("fib_impuestoaplicable", new OptionSetValue(personaMoral.Impuestos));
                }

                if (!string.IsNullOrEmpty(personaMoral.FormaPago))
                {
                    var formaPagoGuid = FindEntityId("fib_formadepago", personaMoral.FormaPago, "fib_codigo", service);

                    if (formaPagoGuid != null && formaPagoGuid != Guid.Empty)
                    {
                        account.Attributes.Add("fib_formadepagoid", new EntityReference("fib_formadepago", formaPagoGuid));
                    }
                }
                

                if (!string.IsNullOrEmpty(personaMoral.UsoComprobante))
                {
                    var usoComprobanteGuid = FindEntityId("fib_usocomprobante", personaMoral.UsoComprobante, "fib_codigo", service);

                    if (usoComprobanteGuid != null && usoComprobanteGuid != Guid.Empty)
                    {
                        account.Attributes.Add("fib_usocomprobanteid", new EntityReference("fib_usocomprobante", usoComprobanteGuid));
                    }
                }

                if (!string.IsNullOrEmpty(personaMoral.RegimenFiscal))
                {
                    var regimenFiscalGuid = FindEntityId("fib_regimenfiscal", personaMoral.RegimenFiscal, "fib_codigo", service);

                    if (regimenFiscalGuid != null && regimenFiscalGuid != Guid.Empty)
                    {
                        account.Attributes.Add("fib_regimenfiscalid", new EntityReference("fib_regimenfiscal", regimenFiscalGuid));
                    }
                }

                if (personaMoral.DomicilioFacturacion != null && !string.IsNullOrEmpty(personaMoral.CodigoPostalV4))
                    account.Attributes.Add("fib_codigopostalcfdi", personaMoral.CodigoPostalV4);

                var createResponse = service.Create(account);

                return new Guid(createResponse.ToString());
            }
            catch (ConectorCRMException ex)
            {
                throw ex;
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception("FB-DL 2: Error al crear la persona Moral - " + personaMoral.RFC, ex);
            }
        }

        public static Guid CrearPersonaMoral(SolidarioObligado personaMoral, IOrganizationService service)
        {

            try
            {
                Entity account = new Entity("account");

                account.Attributes.Add("name", personaMoral.RazonSocial);
                if (!string.IsNullOrEmpty(personaMoral.Nombre))
                {
                    account.Attributes.Add("fib_nombrecomercial", personaMoral.Nombre);
                }
                account.Attributes.Add("fib_solicitante", false);
                account.Attributes.Add("fib_aval", true);
                account.Attributes.Add("fib_accionista", false);
                account.Attributes.Add("fib_rfc", personaMoral.RFC);

                account.Attributes.Add("fib_email", personaMoral.EmailPFE);
                account.Attributes.Add("fib_mobilephone", personaMoral.TelefonoMovil);
                account.Attributes.Add("telephone1", personaMoral.TelefonoOficina);
                account.Attributes.Add("telephone3", personaMoral.TelefonoOficina);

                account.Attributes.Add("emailaddress1", personaMoral.EmailFacturacion);

                string[] nombre = personaMoral.ContactoCobranza.Split(new char[] { ' ' }, 4);
                account.Attributes.Add("address1_primarycontactname", nombre[0]);
                if (nombre.Length > 3)
                {
                    account.Attributes.Add("fib_segundonombre", nombre[1].Trim());
                    account.Attributes.Add("fib_apellidopaterno", nombre[2].Trim());
                    account.Attributes.Add("fib_apellidomaterno", nombre[3].Trim());
                }
                else if (nombre.Length > 2)
                {
                    account.Attributes.Add("fib_apellidopaterno", nombre[1].Trim());
                    account.Attributes.Add("fib_apellidomaterno", nombre[2].Trim());
                }
                //account.Attributes.Add("fib_nombreapoderadolegal", "Prueba");
                //account.Attributes.Add("fib_sexoapoderadolegal", new OptionSetValue(1));
                //account.Attributes.Add("fib_tipodepersonamoral", new OptionSetValue(1));
                if (!string.IsNullOrEmpty(personaMoral.GiroOActividad))
                {
                    var actividadID = ExisteActividadOGiro(personaMoral.GiroOActividad, service);
                    if (actividadID != Guid.Empty)
                        account.Attributes.Add("fib_actividadeconomicapldid", new EntityReference("fib_actividadeconomicapld", actividadID));
                }
                //domicilio
                LlenarDatosDomicilioPersonaMoral(account, personaMoral.DomicilioFacturacion);

                ///probar domicilio operativo
                if (personaMoral.DomicilioOperativo != null)
                {
                    LlenarDatosDomicilioAdicionalPersonaMoral(account, personaMoral.DomicilioOperativo);
                }

                if (!string.IsNullOrEmpty(personaMoral.Nombre))
                {
                    account.Attributes.Add("fib_razonsocialcfdi", personaMoral.Nombre);
                }

                if (personaMoral.Impuestos > 0)
                {
                    account.Attributes.Add("fib_impuestoaplicable", new OptionSetValue(personaMoral.Impuestos));
                }

                if (!string.IsNullOrEmpty(personaMoral.FormaPago))
                {
                    var formaPagoGuid = FindEntityId("fib_formadepago", personaMoral.FormaPago, "fib_codigo", service);

                    if (formaPagoGuid != null && formaPagoGuid != Guid.Empty)
                    {
                        account.Attributes.Add("fib_formadepagoid", new EntityReference("fib_formadepago", formaPagoGuid));
                    }
                }

                if (!string.IsNullOrEmpty(personaMoral.UsoComprobante))
                {
                    var usoComprobanteGuid = FindEntityId("fib_usocomprobante", personaMoral.UsoComprobante, "fib_codigo", service);

                    if (usoComprobanteGuid != null && usoComprobanteGuid != Guid.Empty)
                    {
                        account.Attributes.Add("fib_usocomprobanteid", new EntityReference("fib_usocomprobante", usoComprobanteGuid));
                    }
                }

                if (!string.IsNullOrEmpty(personaMoral.RegimenFiscal))
                {
                    var regimenFiscalGuid = FindEntityId("fib_regimenfiscal", personaMoral.RegimenFiscal, "fib_codigo", service);

                    if (regimenFiscalGuid != null && regimenFiscalGuid != Guid.Empty)
                    {
                        account.Attributes.Add("fib_regimenfiscalid", new EntityReference("fib_regimenfiscal", regimenFiscalGuid));
                    }
                }

                if (personaMoral.DomicilioFacturacion != null && !string.IsNullOrEmpty(personaMoral.CodigoPostalV4))
                    account.Attributes.Add("fib_codigopostalcfdi", personaMoral.CodigoPostalV4);

                var createResponse = service.Create(account);

                return new Guid(createResponse.ToString());
            }
            catch (ConectorCRMException ex)
            {
                throw ex;
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception("FB-DL 2: Error al crear la persona Moral (aval) - " + personaMoral.RFC, ex);
            }
        }


        private static void LlenarDatosDomicilioPersonaMoral(Entity personaMoral, Domicilio domicilio)
        {
            personaMoral.Attributes.Add("address1_line1",
                    string.Format("{0} {1}",
                    domicilio.Calle,
                    (!string.IsNullOrEmpty(domicilio.NumeroInterior) ? "#Int:" + domicilio.NumeroInterior + " " : "")
                    +
                   "#Ext:" + domicilio.NumeroExterior
                    )
                    );
            personaMoral.Attributes.Add("address1_postalcode", domicilio.CodigoPostal);
            personaMoral.Attributes.Add("fib_coloniaid", new EntityReference("fib_colonia", domicilio.DomicilioCodigoPostal.Colonias.First().ColoniaId));
            personaMoral.Attributes.Add("fib_municipioid", new EntityReference("fib_municipio", domicilio.DomicilioCodigoPostal.MunicipioId));
            personaMoral.Attributes.Add("new_estadoid", new EntityReference("new_estado", domicilio.DomicilioCodigoPostal.EstadoId));
            personaMoral.Attributes.Add("fib_ciudadid", new EntityReference("fib_ciudad", domicilio.DomicilioCodigoPostal.CiudadId));
            personaMoral.Attributes.Add("fib_paisid", new EntityReference("new_pais", domicilio.DomicilioCodigoPostal.PaisId));
        }

        private static void LlenarDatosDomicilioAdicionalPersonaMoral(Entity personaMoral, Domicilio domicilio)
        {
            if (domicilio.DomicilioCodigoPostal != null)
            {
                personaMoral.Attributes.Add("fib_domicilioadicional1", true);
                personaMoral.Attributes.Add("address2_line1",
                        string.Format("{0} {1}",
                        domicilio.Calle,
                        (!string.IsNullOrEmpty(domicilio.NumeroInterior) ? "#Int:" + domicilio.NumeroInterior + " " : "")
                        +
                       "#Ext:" + domicilio.NumeroExterior
                        )
                        );
                personaMoral.Attributes.Add("address2_postalcode", domicilio.CodigoPostal);

                personaMoral.Attributes.Add("fib_coloniaid2", new EntityReference("fib_colonia", domicilio.DomicilioCodigoPostal.Colonias.First().ColoniaId));
                personaMoral.Attributes.Add("fib_municipioid2", new EntityReference("fib_municipio", domicilio.DomicilioCodigoPostal.MunicipioId));
                personaMoral.Attributes.Add("fib_estadoid2", new EntityReference("new_estado", domicilio.DomicilioCodigoPostal.EstadoId));
                personaMoral.Attributes.Add("fib_ciudadid2", new EntityReference("fib_ciudad", domicilio.DomicilioCodigoPostal.CiudadId));
                personaMoral.Attributes.Add("fib_paisid2", new EntityReference("new_pais", domicilio.DomicilioCodigoPostal.PaisId));
            }
            else
            {
                personaMoral.Attributes.Add("fib_domicilioadicional1", false);
            }
        }

        #endregion

        #region Solicitud

        private static Guid CrearSolicitud(Solicitud solicitud, Persona persona, IOrganizationService service)
        {
            try
            {
                Entity solicitudEntity = new Entity("opportunity");
                string nombre = "";
                if (persona.PersonaFisica)
                {
                    solicitudEntity.Attributes.Add("customerid", new EntityReference("contact", persona.Id));
                    var entity = service.Retrieve("contact", persona.Id, new ColumnSet(new string[4]
                        {
                          "firstname","middlename","lastname","fib_apellidomaterno"
                        }));
                    nombre = entity.GetAttributeValue<string>("firstname");
                    if (entity.Contains("middlename"))
                    {
                        nombre += " " + entity.GetAttributeValue<string>("middlename");
                    }
                    nombre += " " + entity.GetAttributeValue<string>("lastname");
                    if (entity.Contains("fib_apellidomaterno"))
                    {
                        nombre += " " + entity.GetAttributeValue<string>("fib_apellidomaterno");
                    }
                }
                else if (persona.PersonaMoral)
                {
                    solicitudEntity.Attributes.Add("customerid", new EntityReference("account", persona.Id));
                    var razonSocial = service.Retrieve("account", persona.Id, new ColumnSet(new string[1]
                        {
                          "name"
                        })).GetAttributeValue<string>("name");
                    nombre = razonSocial;
                }

                solicitudEntity.Attributes.Add("fib_descripcion", $"Procaar {nombre} {solicitud.Folio}");
                solicitudEntity.Attributes.Add("fib_antiguedadactividadpreponderante", new OptionSetValue(100000000));
                solicitudEntity.Attributes.Add("transactioncurrencyid", new EntityReference("transactioncurrency", new Guid("0671D47C-D041-DF11-B52A-005056977B10")));
                if (solicitud.DetalleFinanciacion.Importe >= 0)
                {
                    solicitudEntity.Attributes.Add("fib_preciodeventa", new Money(solicitud.DetalleFinanciacion.Importe));
                }
                solicitudEntity.Attributes.Add("fib_saldoafinanciar", new Money(solicitud.DetalleFinanciacion.Importe));
                if (solicitud.DetalleFinanciacion.DepositoGarantia >= 0)
                {
                    solicitudEntity.Attributes.Add("fib_depositoporcumplimiento", new Money(solicitud.DetalleFinanciacion.DepositoGarantia));
                }
                //fallan alguno de los opcionales
                if (solicitud.DetalleFinanciacion.AnticipoComision != 0)
                {
                    solicitudEntity.Attributes.Add("fib_comisionporapertura", solicitud.DetalleFinanciacion.AnticipoComision);
                }
                if (solicitud.DetalleFinanciacion.AnticipoRentaExtraordinaria >= 0)
                {
                    solicitudEntity.Attributes.Add("fib_anticipoderentas", new Money(solicitud.DetalleFinanciacion.AnticipoRentaExtraordinaria));
                }
                if(solicitud.DetalleFinanciacion.ValorResidual != 0)
                {
                    solicitudEntity.Attributes.Add("fib_devau", Convert.ToDouble(solicitud.DetalleFinanciacion.ValorResidual));
                }
                //solicitudEntity.Attributes.Add("fib_devau", 10f);

                solicitudEntity.Attributes.Add("fib_enganche", new Money(0));
                solicitudEntity.Attributes.Add("fib_arrendamientopuro2", new OptionSetValue(1));
                //CONCESIONARIA
                try
                {
                    var concesionariaGuid = ExisteConcesionaria("POR DEFINIR PROCAAR F", service);
                    if(concesionariaGuid != Guid.Empty)
                    {
                        solicitudEntity.Attributes.Add("fib_concesionariaid", new EntityReference("fib_concesionaria", concesionariaGuid));
                    }
                    
                }
                catch (Exception e)
                {
                    //LA CONSESIONARIA NO EXISTE O OCURRIO UN ERROR AL BUSCAR LA CONCESIONARIA
                }
                

                solicitudEntity.Attributes.Add("fib_mensualidadestimada", new Money(solicitud.DetalleFinanciacion.RentaConIva));
                solicitudEntity.Attributes.Add("fib_plazoenmeses2", solicitud.DetalleFinanciacion.Plazo);
                solicitudEntity.Attributes.Add("fib_tipodecrdito", new OptionSetValue(solicitud.TipoCredito));
                //BUSCAR EL TIPO DE PRODUCTO
                try
                {
                    var tipoProductoGuid = ExisteTipoDeProducto("General", service);
                    if (tipoProductoGuid != Guid.Empty)
                    {
                        solicitudEntity.Attributes.Add("fib_tipodeproducto_solicitudid", new EntityReference("fib_tipodeproducto_solicitud", tipoProductoGuid));
                    }
                }
                catch (Exception e)
                {
                    //EL TIPO DE PRODUCTO NO EXISTE O OCURRIO UN ERROR AL BUSCAR EL PRODUCTO
                }

                //if(!string.IsNullOrWhiteSpace(solicitud.Promotor))
                //{
                //    var usuarioSistemaId = ExisteUsuarioSistema(solicitud.Promotor, service);
                //    if (usuarioSistemaId != null && usuarioSistemaId != Guid.Empty)
                //    {
                //        solicitudEntity.Attributes.Add("ownerid", new EntityReference("systemuser", usuarioSistemaId));
                //    }
                //}

                var createResponse = service.Create(solicitudEntity);

                return new Guid(createResponse.ToString());
            }
            catch (ConectorCRMException ex)
            {
                throw ex;
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception("FB-DL 2: Error al crear la solicitud: " + string.Join(" ", ex.Messages()), ex);
            }
        }

        private static void ActualizarSolicitud(Guid solicitudId, DetalleFinanciacion detalleFinanciacion, IOrganizationService service)
        {
            try
            {
                Entity solicitudEntity = new Entity("opportunity");
                solicitudEntity.Id = solicitudId;

                if (detalleFinanciacion.Importe >= 0)
                {
                    solicitudEntity.Attributes.Add("fib_preciodeventa", new Money(detalleFinanciacion.Importe));
                }

                solicitudEntity.Attributes.Add("fib_saldoafinanciar", new Money(detalleFinanciacion.Importe));

                if (detalleFinanciacion.DepositoGarantia >= 0)
                {
                    solicitudEntity.Attributes.Add("fib_depositoporcumplimiento", new Money(detalleFinanciacion.DepositoGarantia));
                }

                if (detalleFinanciacion.AnticipoComision != 0)
                {
                    solicitudEntity.Attributes.Add("fib_comisionporapertura", detalleFinanciacion.AnticipoComision);
                }

                if (detalleFinanciacion.AnticipoRentaExtraordinaria >= 0)
                {
                    solicitudEntity.Attributes.Add("fib_anticipoderentas", new Money(detalleFinanciacion.AnticipoRentaExtraordinaria));
                }

                if (detalleFinanciacion.ValorResidual != 0)
                {
                    solicitudEntity.Attributes.Add("fib_devau", Convert.ToDouble(detalleFinanciacion.ValorResidual));
                }

                solicitudEntity.Attributes.Add("fib_mensualidadestimada", new Money(detalleFinanciacion.RentaConIva));
                solicitudEntity.Attributes.Add("fib_plazoenmeses2", detalleFinanciacion.Plazo);

                service.Update(solicitudEntity);
            }
            catch (ConectorCRMException ex)
            {
                throw ex;
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception("FB-DL 2: Error al actualizar la solicitud", ex);
            }
        }

        public static void AsignarOwnerSolicitud(string userEmail, Guid solicitudUuid, IOrganizationService service)
        {
            try
            {
                var usuarioSistemaId = ExisteUsuarioSistema(userEmail, service);
                if(usuarioSistemaId != null && usuarioSistemaId != Guid.Empty)
                {
                    var assign = new AssignRequest
                    {
                        Assignee = new EntityReference("systemuser", usuarioSistemaId),
                        Target = new EntityReference("opportunity", solicitudUuid)
                    };
                    // Execute the Request
                    service.Execute(assign);
                }
            }
            catch (ConectorCRMException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception("FB-DL 2: Error al asignar el owner en la solicitud", ex);
            }
        }

        public static Entity ExisteSolicitud(string folio, IOrganizationService service)
        {
            String fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                "<entity name=\"fib_bitacorasolicitudescredito\">" +
                                    "<attribute name=\"fib_solicitudcredito\" />" +
                                    "<attribute name=\"createdon\" />" +
                                    "<order attribute=\"createdon\" descending=\"true\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_name\" operator=\"eq\" value=\"" + folio + "\" />" +
                                        "<condition attribute=\"fib_solicitudcredito\" operator=\"not-null\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";
            var results = service.RetrieveMultiple(new FetchExpression(fetchXml));

            var bitacora = results.Entities.Where(x => x.Attributes.Contains("fib_solicitudcredito")).FirstOrDefault();

            Entity solicitudExistente = null;

            if (bitacora != null)
            {
                var solicitudReference = (EntityReference)bitacora.Attributes["fib_solicitudcredito"];

                solicitudExistente = service.Retrieve(solicitudReference.LogicalName, solicitudReference.Id, new ColumnSet(new string[] { "fib_numcredito", "name" }));

                if (solicitudExistente == null || !solicitudExistente.Attributes.Contains("fib_numcredito"))
                {
                    throw new Exception("No se encontró una solicitud con el Folio-IMX: " + folio);
                }
            } else
            {
                throw new Exception("No se encontró una solicitud con el Folio-IMX: " + folio);
            }

            return solicitudExistente;
        }

        #endregion

        #region Avales
        public static Guid CrearAval(Guid Solicitud, Persona persona, IOrganizationService service)
        {

            try
            {
                Entity aval = new Entity("fib_aval");
                string nombre = "";
                if (persona.PersonaFisica)
                {
                    aval.Attributes.Add("fib_tipo", new OptionSetValue(2));
                    aval.Attributes.Add("fib_avalpfid", new EntityReference("contact", persona.Id));
                    var entity = service.Retrieve("contact", persona.Id, new ColumnSet(new string[4]
                        {
                          "firstname","middlename","lastname","fib_apellidomaterno"
                        }));
                    nombre = entity.GetAttributeValue<string>("firstname");
                    if (entity.Contains("middlename"))
                    {
                        nombre += " " + entity.GetAttributeValue<string>("middlename");
                    }
                    nombre += " " + entity.GetAttributeValue<string>("lastname");
                    if (entity.Contains("fib_apellidomaterno"))
                    {
                        nombre += " " + entity.GetAttributeValue<string>("fib_apellidomaterno");
                    }
                    aval.Attributes.Add("fib_relacionconelsolicitante", new OptionSetValue(4));
                    aval.Attributes.Add("fib_otrorelsol", "Por Definir");
                }
                else if (persona.PersonaMoral)
                {
                    aval.Attributes.Add("fib_tipo", new OptionSetValue(1));
                    aval.Attributes.Add("fib_avalpmid", new EntityReference("account", persona.Id));
                    var razonSocial = service.Retrieve("account", persona.Id, new ColumnSet(new string[1]
                        {
                          "name"
                        })).GetAttributeValue<string>("name");
                    nombre = razonSocial;
                }
                aval.Attributes.Add("fib_name", nombre);
                aval.Attributes.Add("fib_opportunityid", new EntityReference("opportunity", Solicitud));

                var createResponse = service.Create(aval);

                return new Guid(createResponse.ToString());
            }
            catch (ConectorCRMException ex)
            {
                throw ex;
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception("FB-DL 2: Error al crear la relacion solicitud aval", ex);
            }
        }

        public static Entity ExisteAvalSolicitud(Guid Solicitud, Persona persona, IOrganizationService service) {
            Entity entity = null;

            String fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                "<entity name=\"fib_aval\">" +
                                    "<attribute name=\"fib_avalpfid\" />" +
                                    "<attribute name=\"fib_avalpmid\" />" +
                                    "<attribute name=\"fib_opportunityid\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_opportunityid\" operator=\"eq\" value=\"" + Solicitud + "\" />" +
                                        $"<condition attribute=\"{(persona.PersonaFisica ? "fib_avalpfid" : "fib_avalpmid")}\" operator=\"eq\" value=\"" + persona.Id + "\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";

            try
            {
                var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (results.Entities.Any())
                {
                    entity = results.Entities[0];
                }
            }
            catch
            {
                throw new ConectorCRMException("FB-CRM 4: Error al recuperar información del cliente");
            }

            return entity;
        }
        #endregion

        #region VALIDACIONES
        private static int ValidarRFC(string rfc, string tipoPersona)
        {
            int persona = 0;
            if (string.IsNullOrEmpty(rfc))
            {
                throw new Exception("El RFC es obligatorio");
            }
            if (string.IsNullOrEmpty(tipoPersona))
            {
                throw new Exception("El Tipo de persona es obligatorio");
            }
            //PERSONAS FISICAS
            if (rfc.Trim().Length == 13 && tipoPersona.ToUpper().Equals("PF"))
            {
                persona = PERSONA_FISICA;
            }
            //PERSONAS MORALES
            if (rfc.Trim().Length == 12 && tipoPersona.ToUpper().Equals("PM"))
            {
                persona = PERSONA_MORAL;
            }

            if (persona == 0)
            {
                throw new Exception("El RFC es invalido.");
            }
            return persona;
        }

        public static bool ValidarTipoCredito(int tipoCredito)
        {
            bool valido = true;
            //VALIDAR TIPO DE CREDITO
            //TO-DO
            //CAMIONES
            var tipoCreditoArray = new List<int> { 1, 2, 3, 4, 5 };
            if (!tipoCreditoArray.Contains(tipoCredito))
            {
                valido = false;
            }
            return valido;
        }
        public static bool ValidarPersonaFisica(PersonaEvaluada personaFisica, IOrganizationService service, out string mensajes)
        {
            bool valida = true;
            mensajes = "";
            //TO-DO VALIDAR GENERO
            var generos = Generos();
            var existeGenero = generos.Where(g => g.Valor.Equals(personaFisica.Genero)).FirstOrDefault();
            if (existeGenero == null)
            {
                mensajes += $"El genero [{personaFisica.Genero}] no existe. ";
            }
            if (string.IsNullOrEmpty(personaFisica.Nombres))
            {
                mensajes += "El Nombre(s) es requerido. ";
            }
            if (string.IsNullOrEmpty(personaFisica.ApellidoPaterno))
            {
                mensajes += "El Apellido Paterno es requerido. ";
            }
            if (string.IsNullOrEmpty(personaFisica.ApellidoMaterno))
            {
                mensajes += "El Apellido Materno es requerido. ";
            }
            if (string.IsNullOrEmpty(personaFisica.FechaNacimiento))
            {
                mensajes += "La fecha de nacimiento es requerida. ";
            }
            else
            {
                if (!DateTime.TryParseExact(personaFisica.FechaNacimiento, "yyyy-MM-dd", new CultureInfo("es-MX"), DateTimeStyles.None, out DateTime result))
                {
                    mensajes += "La fecha de nacimiento tiene formato incorrecto, el formato correcto es: [yyyy-MM-dd] . ";
                }
            }
            if (!string.IsNullOrEmpty(personaFisica.LugarNacimiento))
            {
                //VALIDAR CON CATALOGO
                var datos = ExisteEstado(personaFisica.LugarNacimiento, service);
                if (datos == null)
                {
                    mensajes += $"El lugar de nacimiento [{personaFisica.LugarNacimiento}] no existe. ";
                }
                else
                {
                    personaFisica.PaisNacimientoId = datos.PaisId;
                    personaFisica.EstadoNacimientoId = datos.EstadoId;
                }
            }
            if (personaFisica.Nacionalidad != null)
            {
                var nacionalidades = Nacionalidades();
                //VALIDAR NACIONALIDAD
                var existeNacionalidad = nacionalidades.Where(g => g.Valor.Equals(personaFisica.Nacionalidad)).FirstOrDefault();
                if (existeNacionalidad == null)
                {
                    mensajes += $"La nacionalidad [{personaFisica.Nacionalidad}] no existe. ";
                }
            }
            if (personaFisica.Profesion != null)
            {
                var profesiones = Profesiones();
                //VALIDAR PROFESION
                var existeNacionalidad = profesiones.Where(g => g.Valor.Equals(personaFisica.Profesion)).FirstOrDefault();
                if (existeNacionalidad == null)
                {
                    mensajes += $"La Profesión [{personaFisica.Profesion}] no existe. ";
                }
            }
            if (personaFisica.EstadoCivil != null)
            {
                var estadosCiviles = EstadosCiviles();
                //VALIDAR ESTADO CIVIL
                var existeEstadoCivil = estadosCiviles.Where(g => g.Valor.Equals(personaFisica.EstadoCivil)).FirstOrDefault();
                if (existeEstadoCivil == null)
                {
                    mensajes += $"El estado civil [{personaFisica.EstadoCivil}] no existe. ";
                }
            }
            if (personaFisica.DocumentoIdentificacion != null)
            {
                var documentos = DocumentosIdentificacion();
                //VALIDAR DOCUMENTOS
                var existeDocumento = documentos.Where(g => g.Valor.Equals(personaFisica.DocumentoIdentificacion)).FirstOrDefault();
                if (existeDocumento == null)
                {
                    mensajes += $"El Documento de Indentificacion [{personaFisica.DocumentoIdentificacion}] no existe. ";
                }
            }
            //REVISAR SI NOMBRE LARGO SI SE CONCATENAN LOS CAMPOS NOMBRE Y SEGUNDO NOMBRE
            if (string.IsNullOrEmpty(personaFisica.EmailPFE))
            {
                mensajes += "El EmailPFE es requerido. ";
            }
            if (string.IsNullOrEmpty(personaFisica.TelefonoMovil))
            {
                mensajes += "El Teléfono Móvil es requerido";
            }
            if (!string.IsNullOrEmpty(personaFisica.TelefonoMovil) && personaFisica.TelefonoMovil.Trim().Length > 14)
            {
                mensajes += "El Teléfono Móvil no puede ser mayor a 14 Caracteres. ";
            }
            if (!string.IsNullOrEmpty(personaFisica.GiroOActividad))
            {
                //VALIDAR GIRO O ACTIVIDAD
                var actividadID = ExisteActividadOGiro(personaFisica.GiroOActividad, service);
                if (actividadID != Guid.Empty)
                {
                    personaFisica.ActividadId = actividadID;
                }
            }
            //VALIDAR DOMICILIO FACTURACION
            var domicilioFactValido = ValidarDomicilio(personaFisica.DomicilioFacturacion, service, out string mensajesDomFacturacion);
            if (!domicilioFactValido)
            {
                mensajes += "Errores Domicilio Facturación: " + mensajesDomFacturacion + "";
            }

            ValidarSegundoDomicilio(personaFisica.DomicilioOperativo, service);
            /*if (!domicilioOperativo)
            {
                mensajes += "Errores Domicilio Operativo: " + mensajesDomOperativo + " ";
            }*/
            if (!string.IsNullOrEmpty(mensajes))
            {
                valida = false;
            }
            return valida;
        }
        public static bool ValidarSolidarioObligadoPF(SolidarioObligado personaFisica, IOrganizationService service, out string mensajes)
        {
            bool valida = true;
            mensajes = "";
            //TO-DO VALIDAR GENERO
            var generos = Generos();
            var existeGenero = generos.Where(g => g.Valor.Equals(personaFisica.Genero)).FirstOrDefault();
            if (existeGenero == null)
            {
                mensajes += $"El genero [{personaFisica.Genero}] no existe. ";
            }
            if (string.IsNullOrEmpty(personaFisica.Nombres))
            {
                mensajes += "El campo Nombres es requerido. ";
            }
            if (string.IsNullOrEmpty(personaFisica.ApellidoPaterno))
            {
                mensajes += "El Apellido Paterno es requerido. ";
            }
            if (string.IsNullOrEmpty(personaFisica.ApellidoMaterno))
            {
                mensajes += "El Apellido Materno es requerido. ";
            }
            if (!string.IsNullOrEmpty(personaFisica.LugarNacimiento))
            {
                //VALIDAR CON CATALOGO
                var datos = ExisteEstado(personaFisica.LugarNacimiento, service);
                if (datos == null)
                {
                    mensajes += $"El lugar de nacimiento [{personaFisica.LugarNacimiento}] no existe. ";
                }
                else
                {
                    personaFisica.PaisNacimientoId = datos.PaisId;
                    personaFisica.EstadoNacimientoId = datos.EstadoId;
                }
            }
            if (personaFisica.Nacionalidad != null)
            {
                var nacionalidades = Nacionalidades();
                //VALIDAR NACIONALIDAD
                var existeNacionalidad = nacionalidades.Where(g => g.Valor.Equals(personaFisica.Nacionalidad)).FirstOrDefault();
                if (existeNacionalidad == null)
                {
                    mensajes += $"La nacionalidad [{personaFisica.Nacionalidad}] no existe. ";
                }
            }
            if (personaFisica.Profesion != null)
            {
                var profesiones = Profesiones();
                //VALIDAR PROFESION
                var existeNacionalidad = profesiones.Where(g => g.Valor.Equals(personaFisica.Profesion)).FirstOrDefault();
                if (existeNacionalidad == null)
                {
                    mensajes += $"La nacionalidad [{personaFisica.Profesion}] no existe. ";
                }
            }
            if (personaFisica.EstadoCivil != null)
            {
                var estadosCiviles = EstadosCiviles();
                //VALIDAR ESTADO CIVIL
                var existeEstadoCivil = estadosCiviles.Where(g => g.Valor.Equals(personaFisica.EstadoCivil)).FirstOrDefault();
                if (existeEstadoCivil == null)
                {
                    mensajes += $"El estado civil [{personaFisica.EstadoCivil}] no existe. ";
                }
            }
            if (personaFisica.DocumentoIdentificacion != null)
            {
                var documentos = DocumentosIdentificacion();
                //VALIDAR DOCUMENTOS
                var existeDocumento = documentos.Where(g => g.Valor.Equals(personaFisica.DocumentoIdentificacion)).FirstOrDefault();
                if (existeDocumento == null)
                {
                    mensajes += $"El Documento de Indentificacion [{personaFisica.DocumentoIdentificacion}] no existe. ";
                }
            }
            //REVISAR SI NOMBRE LARGO SI SE CONCATENAN LOS CAMPOS NOMBRE Y SEGUNDO NOMBRE
            if (string.IsNullOrEmpty(personaFisica.EmailPFE))
            {
                mensajes += "El EmailPFE es requerido. ";
            }
            if (string.IsNullOrEmpty(personaFisica.TelefonoMovil))
            {
                mensajes += "El Teléfono Móvil es requerido";
            }
            if (!string.IsNullOrEmpty(personaFisica.TelefonoMovil) && personaFisica.TelefonoMovil.Trim().Length > 14)
            {
                mensajes += "El Teléfono Móvil no puede ser mayor a 14 Caracteres. ";
            }
            if (!string.IsNullOrEmpty(personaFisica.GiroOActividad))
            {
                //VALIDAR GIRO O ACTIVIDAD
                var actividadID = ExisteActividadOGiro(personaFisica.GiroOActividad, service);
                if (actividadID != Guid.Empty)
                {
                    personaFisica.ActividadId = actividadID;
                }
            }
            //VALIDAR DOMICILIO FACTURACION
            var domicilioFactValido = ValidarDomicilio(personaFisica.DomicilioFacturacion, service, out string mensajesDomFacturacion);
            if (!domicilioFactValido)
            {
                mensajes += "Errores Domicilio Facturación: " + mensajesDomFacturacion + " ";
            }

            ValidarSegundoDomicilio(personaFisica.DomicilioOperativo, service);
            /*if (!domicilioOperativo)
            {
                mensajes += "Errores Domicilio Operativo: " + mensajesDomOperativo + " ";
            }*/
            if (!string.IsNullOrEmpty(mensajes))
            {
                valida = false;
            }
            return valida;
        }

        private static bool ValidarDomicilio(Domicilio domicilio, IOrganizationService service, out string mensajes)
        {
            bool valido = true;
            mensajes = "";
            if (string.IsNullOrEmpty(domicilio.Calle))
            {
                mensajes += "La Calle es Requerida. ";
            }
            if (string.IsNullOrEmpty(domicilio.NumeroExterior))
            {
                mensajes += "El Número Exterior es Requerido. ";
            }
            if (string.IsNullOrEmpty(domicilio.CodigoPostal))
            {
                mensajes += "El Codigo Postal es Requerido. ";
            }
            else
            {
                //VALIDAR QUE EL CODIGO POSTAL EXISTA
                var domicilioCodigoPostal = ExisteCodigoPostal(domicilio.CodigoPostal, service);
                if (domicilioCodigoPostal == null)
                {
                    mensajes += "El Codigo Postal no existe. ";
                }
                else
                {
                    domicilio.DomicilioCodigoPostal = domicilioCodigoPostal;
                }
            }
            if (string.IsNullOrEmpty(domicilio.Colonia))
            {
                mensajes += "La Colonia es Requerida. ";
            }
            /*else
            {
                //VALIDAR COLONIA
                if(domicilio.DomicilioCodigoPostal != null)
                {
                    var colonia = domicilio.DomicilioCodigoPostal.Colonias.Where(c => c.Name.StartsWith(domicilio.Colonia.Trim()+" ")).OrderBy(d=>d.Name).FirstOrDefault();
                    if(colonia == null)
                    {
                        var colonias = string.Join(" ", domicilio.DomicilioCodigoPostal.Colonias.Select(c => c.Name).ToArray());
                        mensajes += "La Colonia no existe. Colonias Disponibles: "+colonias;
                    }
                    else
                    {
                        domicilio.ColoniaId = colonia.ColoniaId;
                    }
                }
            }
            */

            if (!string.IsNullOrEmpty(mensajes))
            {
                valido = false;
            }
            return valido;
        }

        private static void ValidarSegundoDomicilio(Domicilio domicilio, IOrganizationService service)
        {   
            if (!string.IsNullOrEmpty(domicilio.CodigoPostal))
            {
                //VALIDAR QUE EL CODIGO POSTAL EXISTA
                var domicilioCodigoPostal = ExisteCodigoPostal(domicilio.CodigoPostal, service);
                if (domicilioCodigoPostal != null)
                {
                    domicilio.DomicilioCodigoPostal = domicilioCodigoPostal;
                }
            }
            
        }

        public static bool ValidarPersonaMoral(PersonaEvaluada personaMoral, IOrganizationService service, out string mensajes)
        {
            bool valida = true;
            mensajes = "";
            if (string.IsNullOrEmpty(personaMoral.Nombre))
            {
                mensajes += "El Nombre es requerido. ";
            }
            if (string.IsNullOrEmpty(personaMoral.RazonSocial))
            {
                mensajes += "La Razon Social es requerida. ";
            }
            //REVISAR SI NOMBRE LARGO SI SE CONCATENAN LOS CAMPOS NOMBRE Y SEGUNDO NOMBRE
            if (string.IsNullOrEmpty(personaMoral.EmailPFE))
            {
                mensajes += "El EmailPFE es requerido. ";
            }
            if (string.IsNullOrEmpty(personaMoral.TelefonoMovil) || personaMoral.TelefonoMovil.Trim().Length > 14)
            {
                mensajes += "El Teléfono Móvil es Requerido y no puede ser mayor a 14 Caracteres. ";
            }
            if (string.IsNullOrEmpty(personaMoral.TelefonoOficina))
            {
                mensajes += "El Teléfono Oficina es Requerido. ";
            }
            if (string.IsNullOrEmpty(personaMoral.EmailFacturacion))
            {
                mensajes += "El Email de Facturación es Requerido. ";
            }
            //CONTACTO COBRANZA
            if (string.IsNullOrEmpty(personaMoral.ContactoCobranza))
            {
                mensajes += "El Contacto Cobranza es Requerido. ";
            }
            if (!string.IsNullOrEmpty(personaMoral.GiroOActividad))
            {
                //VALIDAR GIRO O ACTIVIDAD
                var actividadID = ExisteActividadOGiro(personaMoral.GiroOActividad, service);
                if (actividadID != Guid.Empty)
                {
                    personaMoral.ActividadId = actividadID;
                }
            }
            //VALIDAR DOMICILIO FACTURACION
            var domicilioFactValido = ValidarDomicilio(personaMoral.DomicilioFacturacion, service, out string mensajesDomFacturacion);
            if (!domicilioFactValido)
            {
                mensajes += "Errores Domicilio Facturación: " + mensajesDomFacturacion + " ";
            }

            ValidarSegundoDomicilio(personaMoral.DomicilioOperativo, service);
            /*if (!domicilioOperativo)
            {
                mensajes += "Errores Domicilio Operativo: " + mensajesDomOperativo + " ";
            }*/
            if (!string.IsNullOrEmpty(mensajes))
            {
                valida = false;
            }
            return valida;
        }

        public static bool ValidarSolidarioObligadoPM(SolidarioObligado personaMoral, IOrganizationService service, out string mensajes)
        {
            bool valida = true;
            mensajes = "";
            if (string.IsNullOrEmpty(personaMoral.Nombre))
            {
                mensajes += "El Nombre es requerido. ";
            }
            if (string.IsNullOrEmpty(personaMoral.RazonSocial))
            {
                mensajes += "La Razon Social es requerida. ";
            }
            //REVISAR SI NOMBRE LARGO SI SE CONCATENAN LOS CAMPOS NOMBRE Y SEGUNDO NOMBRE
            if (string.IsNullOrEmpty(personaMoral.EmailPFE))
            {
                mensajes += "El EmailPFE es requerido. ";
            }
            if (string.IsNullOrEmpty(personaMoral.TelefonoMovil) || personaMoral.TelefonoMovil.Trim().Length > 14)
            {
                mensajes += "El Teléfono Móvil es Requerido y no puede ser mayor a 14 Caracteres. ";
            }
            if (string.IsNullOrEmpty(personaMoral.TelefonoOficina))
            {
                mensajes += "El Teléfono Oficina es Requerido. ";
            }
            if (string.IsNullOrEmpty(personaMoral.EmailFacturacion))
            {
                mensajes += "El Email de Facturación es Requerido. ";
            }
            //CONTACTO COBRANZA
            if (string.IsNullOrEmpty(personaMoral.ContactoCobranza))
            {
                mensajes += "El Contacto Cobranza es Requerido. ";
            }
            if (!string.IsNullOrEmpty(personaMoral.GiroOActividad))
            {
                //VALIDAR GIRO O ACTIVIDAD
                var actividadID = ExisteActividadOGiro(personaMoral.GiroOActividad, service);
                if (actividadID != Guid.Empty)
                {
                    personaMoral.ActividadId = actividadID;
                }
            }
            //VALIDAR DOMICILIO FACTURACION
            var domicilioFactValido = ValidarDomicilio(personaMoral.DomicilioFacturacion, service, out string mensajesDomFacturacion);
            if (!domicilioFactValido)
            {
                mensajes += "Errores Domicilio Facturación: " + mensajesDomFacturacion + " ";
            }

            ValidarSegundoDomicilio(personaMoral.DomicilioOperativo, service);
            /*if (!domicilioOperativo)
            {
                mensajes += "Errores Domicilio Operativo: " + mensajesDomOperativo + " ";
            }*/
            if (!string.IsNullOrEmpty(mensajes))
            {
                valida = false;
            }
            return valida;
        }

        public static bool ValidarSolidariosObligados(List<SolidarioObligado> solidariosObligados, IOrganizationService service, out string mensajes)
        {
            int numAval = 1;
            mensajes = "";
            bool valida = true;
            foreach (var solidario in solidariosObligados)
            {
                string mensajesAvales = "";
                int personaAval = 0;
                try
                {
                    personaAval = ValidarRFC(solidario.RFC, solidario.TipoPersona);
                }
                catch (Exception e)
                {
                    mensajesAvales += e.Message;
                    personaAval = 0;
                }
                if (personaAval == PERSONA_FISICA)
                {
                    //VALIDAR DATOS DE LA PERSONA FISICA
                    var datosValidos = ValidarSolidarioObligadoPF(solidario, service, out string erroresSolidario);
                    if (!datosValidos)
                    {
                        mensajesAvales += "Errores en datos del Aval: " + erroresSolidario;
                    }
                }
                else if (personaAval == PERSONA_MORAL)
                {
                    //VALIDAR DATOS DE LA PERSONA MORAL
                    var datosValidos = ValidarSolidarioObligadoPM(solidario, service, out string erroresPersonaFisica);
                    if (!datosValidos)
                    {
                        mensajes += "Errores en datos del aval: " + erroresPersonaFisica;
                    }
                }
                if (!string.IsNullOrEmpty(mensajesAvales))
                {
                    mensajes += string.Format("Aval #{0}: {1}", numAval, mensajesAvales);
                }
                numAval++;
            }

            if (!string.IsNullOrEmpty(mensajes))
            {
                valida = false;
            }
            return valida;
        }

        private static bool ValidarDatosFinanciacion(DetalleFinanciacion detalle, out string mensajes)
        {
            bool valido = true;
            mensajes = "";
            if (detalle.Importe < 0)
            {
                mensajes += "El Importe debe ser mayor a cero";
            }
            if (detalle.DepositoGarantia <= 0)
            {
                //mensajes += "El Deposito de Garantia debe ser mayor a cero";
            }
            if (detalle.AnticipoComision < 0)
            {
                mensajes += "El Anticipo como Comisión debe ser mayor a cero";
            }
            if (detalle.AnticipoRentaExtraordinaria < 0)
            {
                mensajes += "El Anticipo como Renta Extraordinaria debe ser mayor a cero";
            }
            if (detalle.RentaConIva < 0)
            {
                mensajes += "La Renta con Iva debe ser mayor a cero";
            }
            if (detalle.Plazo < 0)
            {
                mensajes += "El plazo en meses debe ser mayor a cero";
            }
            if (detalle.ValorResidual < 0 || detalle.ValorResidual > 100)
            {
                mensajes += "El valor residual debe estar entre [0-100]";
            }
            if (!string.IsNullOrEmpty(mensajes))
            {
                valido = false;
            }
            return valido;
        }
        private static bool ValidarDatos(Solicitud solicitud, IOrganizationService service, out string mensajes)
        {
            bool validos = true;
            mensajes = "";
            if (string.IsNullOrEmpty(solicitud.Folio))
            {
                mensajes += "El folio es requerido. ";
            }
            if (!ValidarTipoCredito(solicitud.TipoCredito))
            {
                mensajes += "El tipo de Crédito no es valido. ";
            }
            //VALIDAR SOLICITANTE
            if (solicitud.PersonaEvaluada == null)
            {
                mensajes += "Los datos del solicitante son requerido. ";
            }
            else
            {
                int personaSolicitante = 0;
                try
                {
                    personaSolicitante = ValidarRFC(solicitud.PersonaEvaluada.RFC, solicitud.PersonaEvaluada.TipoPersona);
                }
                catch (Exception e)
                {
                    mensajes += " Solicitante:" + e.Message + " ";
                }
                if (personaSolicitante == PERSONA_FISICA)
                {
                    //VALIDAR DATOS DE LA PERSONA FISICA
                    var datosValidos = ValidarPersonaFisica(solicitud.PersonaEvaluada, service, out string erroresPersonaFisica);
                    if (!datosValidos)
                    {
                        mensajes += "Errores en datos del solicitante: " + erroresPersonaFisica;
                    }
                }
                else if (personaSolicitante == PERSONA_MORAL)
                {
                    //VALIDAR DATOS DE LA PERSONA MORAL
                    var datosValidos = ValidarPersonaMoral(solicitud.PersonaEvaluada, service, out string erroresPersonaFisica);
                    if (!datosValidos)
                    {
                        mensajes += "Errores en datos del solicitante: " + erroresPersonaFisica;
                    }
                }

                //VALIDAR AVALES
                if(solicitud.PersonaEvaluada.SolidariosObligados != null && solicitud.PersonaEvaluada.SolidariosObligados.Any())
                {
                    var solidariosValidos = ValidarSolidariosObligados(solicitud.PersonaEvaluada.SolidariosObligados, service, out string erroresSolidariosObligados);
                    if(!solidariosValidos)
                    {
                        mensajes += erroresSolidariosObligados;
                    }
                }
            }
            if (solicitud.DetalleFinanciacion == null)
            {
                mensajes += "El detalle de Financiacion es Requerido ";
            }
            else
            {
                var valido = ValidarDatosFinanciacion(solicitud.DetalleFinanciacion, out string mensajesFinan);
                if (!valido)
                {
                    mensajes += "Errores en datos del Detalle de la Financiación: " + mensajesFinan;
                }
            }
            if (!string.IsNullOrEmpty(mensajes))
            {
                validos = false;
            }
            return validos;
        }
        #endregion

        #region LISTAS_PF_PM
        public static List<ValorLista> Profesiones()
        {
            var lista = new List<ValorLista>
            {
                new ValorLista { Nombre = "Doctores", Valor = 1 },
                new ValorLista { Nombre = "Maestros", Valor = 2 },
                new ValorLista { Nombre = "Contadores", Valor = 3 },
                new ValorLista { Nombre = "Abogados", Valor = 4 },
                new ValorLista { Nombre = "Militares", Valor = 5 },
                new ValorLista { Nombre = "Otros", Valor = 6 }
            };
            return lista;
        }

        public static List<ValorLista> TipoPersonaFisica()
        {
            var lista = new List<ValorLista>
            {
                new ValorLista { Nombre = "Con Actividad Empresarial", Valor = 200000 },
                new ValorLista { Nombre = "Persona Física", Valor = 200002 }
            };
            return lista;
        }

        public static List<ValorLista> EstadosCiviles()
        {
            var lista = new List<ValorLista>
            {
                new ValorLista { Nombre = "Casado Bienes Mancomunados", Valor = 1 },
                new ValorLista { Nombre = "Casado Bienes Separados", Valor = 2 },
                new ValorLista { Nombre = "Soltero", Valor = 3 },
                new ValorLista { Nombre = "Unión Libre", Valor = 4 },
                new ValorLista { Nombre = "Divorciado", Valor = 200001 },
                new ValorLista { Nombre = "Viudo", Valor = 200002 }
            };
            return lista;
        }

        public static List<ValorLista> DocumentosIdentificacion()
        {
            var lista = new List<ValorLista>
            {
                new ValorLista { Nombre = "IFE", Valor = 1 },
                new ValorLista { Nombre = "Pasaporte", Valor = 2 }
            };
            return lista;
        }

        public static List<ValorLista> Generos()
        {
            var lista = new List<ValorLista>
            {
                new ValorLista { Nombre = "Masculino", Valor = 1 },
                new ValorLista { Nombre = "Femenino", Valor = 2 }
            };
            return lista;
        }

        public static List<ValorLista> Nacionalidades()
        {
            var lista = new List<ValorLista>
            {
                new ValorLista { Nombre = "Mexicano", Valor = 1 },
                new ValorLista { Nombre = "Extranjero", Valor = 2 },
                new ValorLista { Nombre = "Doble nacionalidad", Valor = 3 }
            };
            return lista;
        }

        #endregion

        #region Catalogos
        public static DomicilioCodigoPostal ExisteCodigoPostal(string codigoPostal, IOrganizationService service)
        {
            DomicilioCodigoPostal domicilio = null;

            String fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                "<entity name=\"fib_colonia\">" +
                                    "<attribute name=\"fib_coloniaid\" />" +
                                    "<attribute name=\"fib_pasid\" />" +
                                    "<attribute name=\"fib_estadoid\" />" +
                                    "<attribute name=\"fib_delegacinomunicipioid\" />" +
                                    "<attribute name=\"fib_cp\" />" +
                                    "<attribute name=\"fib_ciudadid\" />" +
                                    "<attribute name=\"fib_name\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_cp\" operator=\"eq\" value=\"" + codigoPostal + "\" />" +
                                        "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";

            try
            {
                var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (results.Entities.Any())
                {
                    var result = results.Entities[0];
                    domicilio = new DomicilioCodigoPostal()
                    {
                        //result.AtributoColleccion("fib_coloniaid",TipoAtributos.ENTITY_REFERENCE_ID).ToString()
                        PaisId = new Guid(result.AtributoColleccion("fib_pasid", TipoAtributos.ENTITY_REFERENCE_ID).ToString()),
                        Pais = result.AtributoColleccion("fib_pasid", TipoAtributos.ENTITY_REFERENCE_NAME).ToString(),
                        EstadoId = new Guid(result.AtributoColleccion("fib_estadoid", TipoAtributos.ENTITY_REFERENCE_ID).ToString()),
                        Estado = result.AtributoColleccion("fib_estadoid", TipoAtributos.ENTITY_REFERENCE_NAME).ToString(),
                        CiudadId = new Guid(result.AtributoColleccion("fib_ciudadid", TipoAtributos.ENTITY_REFERENCE_ID).ToString()),
                        Ciudad = result.AtributoColleccion("fib_ciudadid", TipoAtributos.ENTITY_REFERENCE_NAME).ToString(),
                        MunicipioId = new Guid(result.AtributoColleccion("fib_delegacinomunicipioid", TipoAtributos.ENTITY_REFERENCE_ID).ToString()),
                        Municipio = result.AtributoColleccion("fib_delegacinomunicipioid", TipoAtributos.ENTITY_REFERENCE_NAME).ToString(),
                        CodigoPostal = result.AtributoColleccion("fib_cp", TipoAtributos.STRING).ToString(),
                    };
                    domicilio.Colonias = new List<DomicilioColonia>();
                    foreach (var entity in results.Entities)
                    {
                        domicilio.Colonias.Add(new DomicilioColonia()
                        {
                            ColoniaId = new Guid(result.AtributoColleccion("fib_coloniaid", TipoAtributos.STRING).ToString()),
                            Name = result.AtributoColleccion("fib_name", TipoAtributos.STRING).ToString(),
                        });
                    }
                }
            }
            catch
            {
                throw new ConectorCRMException("FB-CRM 4: Error al recuperar información del municipio");
            }

            return domicilio;
        }

        public static DomicilioCodigoPostal ExisteEstado(string estado, IOrganizationService service)
        {
            DomicilioCodigoPostal datos = null;

            String fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                "<entity name=\"new_estado\">" +
                                    "<attribute name=\"new_pais\" />" +
                                    "<attribute name=\"new_estadoid\" />" +
                                    "<filter>" +
                                    "<filter type=\"or\">" +
                                        "<condition attribute=\"new_estado\" operator=\"like\" value=\"%" + estado + "%\" />" +
                                    "</filter>" +
                                    "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";

            try
            {
                var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (results.Entities.Any())
                {
                    var result = results.Entities[0];
                    datos = new DomicilioCodigoPostal()
                    {
                        PaisId = new Guid(result.AtributoColleccion("new_pais", TipoAtributos.ENTITY_REFERENCE_ID).ToString()),
                        EstadoId = new Guid(result.AtributoColleccion("new_estadoid").ToString())
                    };
                }
            }
            catch
            {
                throw new ConectorCRMException("FB-CRM 4: Error al recuperar información del pais");
            }

            return datos;
        }
        public static Guid ExisteActividadOGiro(string codigoActividad, IOrganizationService service)
        {
            Guid actividadId = Guid.Empty;

            String fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                "<entity name=\"fib_actividadeconomicapld\">" +
                                    "<attribute name=\"fib_actividadeconomicapldid\" />" +
                                    "<filter>" +
                                    "<filter type=\"or\">" +
                                    "<condition attribute=\"fib_codigo\" operator=\"eq\" value=\"" + codigoActividad + "\" />" +
                                    "<condition attribute=\"fib_name\" operator=\"eq\" value=\"" + codigoActividad + "\" />" +
                                    "</filter>" +
                                    "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";

            try
            {
                var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (results.Entities.Any())
                {
                    var result = results.Entities[0];
                    actividadId = new Guid(result.AtributoColleccion("fib_actividadeconomicapldid").ToString());
                }
            }
            catch
            {
                throw new ConectorCRMException("FB-CRM 4: Error al recuperar información de la actividad");
            }

            return actividadId;
        }

        private static string NumeroPersona(Persona persona, IOrganizationService service)
        {
            string numero = "";
            if (persona.PersonaFisica)
            {
                numero = service.Retrieve("contact", persona.Id, new ColumnSet(new string[1]
                {
                    "fib_numpersonafisica"
                })).GetAttributeValue<string>("fib_numpersonafisica");
            }
            else if (persona.PersonaMoral)
            {
                numero = service.Retrieve("account", persona.Id, new ColumnSet(new string[1]
                {
                    "accountnumber"
                })).GetAttributeValue<string>("accountnumber");
            }
            return numero;
        }

        public static Guid ExisteConcesionaria(string concesionaria, IOrganizationService service)
        {
            Guid concesionariaId = Guid.Empty;

            String fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                "<entity name=\"fib_concesionaria\">" +
                                    "<attribute name=\"fib_concesionariaid\" />" +
                                    "<filter>" +
                                    "<condition attribute=\"fib_name\" operator=\"eq\" value=\"" + concesionaria + "\" />" +
                                    "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";

            try
            {
                var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (results.Entities.Any())
                {
                    var result = results.Entities[0];
                    concesionariaId = new Guid(result.AtributoColleccion("fib_concesionariaid").ToString());
                }
            }
            catch
            {
                throw new ConectorCRMException("FB-CRM 4: Error al recuperar información de la concesionaria");
            }

            return concesionariaId;
        }

        public static Guid ExisteTipoDeProducto(string tipoProducto, IOrganizationService service)
        {
            Guid tipoProductoId = Guid.Empty;

            String fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                "<entity name=\"fib_tipodeproducto_solicitud\">" +
                                    "<attribute name=\"fib_tipodeproducto_solicitudid\" />" +
                                    "<filter>" +
                                    "<condition attribute=\"fib_name\" operator=\"eq\" value=\"" + tipoProducto + "\" />" +
                                    "<condition attribute=\"statecode\" operator=\"eq\" value=\"0\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";

            try
            {
                var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (results.Entities.Any())
                {
                    var result = results.Entities[0];
                    tipoProductoId = new Guid(result.AtributoColleccion("fib_tipodeproducto_solicitudid").ToString());
                }
            }
            catch
            {
                throw new ConectorCRMException("FB-CRM 4: Error al recuperar información el tipo de producto");
            }

            return tipoProductoId;
        }

        #endregion

        #region Usuarios Sistema
        public static Guid ExisteUsuarioSistema(string compareValue, IOrganizationService service, string compareAttribute = "internalemailaddress")
        {
            Guid userId = Guid.Empty;

            String fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                "<entity name=\"systemuser\">" +
                                    "<attribute name=\"systemuserid\" />" +
                                    "<filter>" +
                                        $"<condition attribute=\"{compareAttribute}\" operator=\"eq\" value=\"" + compareValue + "\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";

            try
            {
                var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (results.Entities.Any())
                {
                    userId = new Guid(results.Entities[0].AtributoColleccion("systemuserid", TipoAtributos.STRING).ToString());
                }
            }
            catch
            {
                throw new ConectorCRMException("FB-CRM 4: Error al recuperar información del usuario");
            }

            return userId;
        }

        public static Guid FindEntityId(string entityName, string compareValue, string compareAttribute, IOrganizationService service)
        {
            Guid entityId = Guid.Empty;

            String fetchXml = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                $"<entity name=\"{entityName}\">" +
                                    "<filter>" +
                                        $"<condition attribute=\"{compareAttribute}\" operator=\"eq\" value=\"" + compareValue + "\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";

            try
            {
                var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
                if (results.Entities.Any())
                {
                    entityId = results.Entities.FirstOrDefault().Id;
                }
            }
            catch
            {
                throw new ConectorCRMException($"FB-CRM 4: Error al recuperar información de la entidad {entityName}");
            }

            return entityId;
        }
        #endregion

        #region Documentos de solicitud
        public static DocumentoSolicitudResponse ProcesarDocumentoSolicitud(DocumentoSolicitudRequest documentoSolicitud)
        {
            var service = CRMService.createService();
            var response = new DocumentoSolicitudResponse();
            Guid idBitacora = CrearBitacoraDocumento(documentoSolicitud.Folio, documentoSolicitud.Documento.Nombre, documentoSolicitud.UltimoDocumento, service);
            var mensajesBitacora = "";

            try
            {
                string user = ConfigurationManager.AppSettings["SPUser"];
                string password = ConfigurationManager.AppSettings["SPPassword"];
                string domain = ConfigurationManager.AppSettings["SPDomain"];
                string spUrl = ConfigurationManager.AppSettings["SharePointURL"];
                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                NetworkCredential credential = new NetworkCredential(user, password, domain);

                var ultimaBitacora = ObtenerUltimaBitacora(documentoSolicitud.Folio, service);
                AsignarBitacoraSolicitudBitacoraDocumento(idBitacora, ultimaBitacora.Id, service);

                var solicitudEntity = ExisteSolicitud(documentoSolicitud.Folio, service);
                AsignarSolicitudBitacoraDocumento(idBitacora, solicitudEntity.Id, service);
                
                var solicitudFolderName = solicitudEntity.GetAttributeValue<string>("fib_numcredito") + "_" + solicitudEntity.Id.ToString("N").ToUpper();

                var SPDocumentLocation = ObtenerCRMSharePointDocumentLocationByRegardingId(solicitudEntity.Id, service);

                if(SPDocumentLocation == null || SPDocumentLocation.Id == Guid.Empty)
                {
                    var strFolderSolicitudUrl = $"{spUrl}/opportunity/{solicitudFolderName}";

                    CreateFolderSP(strFolderSolicitudUrl, credential);
                    CreateFolderCRM(solicitudFolderName, solicitudEntity.Id, service);
                    mensajesBitacora += "Carpeta SharePoint Creada, ";
                } else
                {
                    solicitudFolderName = SPDocumentLocation.GetAttributeValue<string>("relativeurl");
                }

                SPFileUpload.Credenciales spCredential = new SPFileUpload.Credenciales();
                spCredential.Usuario = user;
                spCredential.Contrasenia = password;
                spCredential.Dominio = domain;
                spCredential.Host = Environment.MachineName;
                spCredential.IPAddress = Dns.GetHostAddresses(Environment.MachineName)[0].ToString();

                SPFileUpload.SPFileUploadClient spService = new SPFileUpload.SPFileUploadClient();

                SPFileUpload.FileCreationInformation file = new SPFileUpload.FileCreationInformation();
                file.Content = Convert.FromBase64String(documentoSolicitud.Documento.Archivo);
                file.Url = $"{documentoSolicitud.Documento.Nombre}";
                file.Overwrite = true;

                spService.CargaArchivo(spCredential, spUrl, "opportunity", false, solicitudFolderName, file);

                mensajesBitacora += "Archivo cargado con éxito, ";

                response.Success = true;
                response.Uri = $"{spUrl}/opportunity/{solicitudFolderName}/{documentoSolicitud.Documento.Nombre}";
                response.Message = "";

                if (documentoSolicitud.UltimoDocumento)
                {
                    SendMail(solicitudEntity.Id, documentoSolicitud.Folio, ultimaBitacora, service, out string errorsMail);
                    if(!string.IsNullOrEmpty(errorsMail))
                    {
                        mensajesBitacora += errorsMail;
                    } else
                    {
                        mensajesBitacora += "Notificación por correo enviada";
                    }
                }

                ActualizarBitacoraDocumento(idBitacora, mensajesBitacora, service);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Uri = null;
                response.Message = ex.Message;

                mensajesBitacora += "Error al cargar archivo: " + ex.Message;

                ActualizarBitacoraDocumento(idBitacora, mensajesBitacora, service);
            }

            return response;
        }

        private static void CreateFolderSP(string folderUrl, NetworkCredential credentials)
        {
            try
            {
                WebRequest requestCreateFolderSolicitud = WebRequest.Create(folderUrl);
                requestCreateFolderSolicitud.Credentials = credentials;
                requestCreateFolderSolicitud.PreAuthenticate = true;
                requestCreateFolderSolicitud.Method = WebRequestMethods.Http.MkCol;
                WebResponse responseCreateFolderSolicitud = requestCreateFolderSolicitud.GetResponse();
                responseCreateFolderSolicitud.Close();
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(System.Net.WebException))
                {
                    var webException = (WebException)ex;
                    if(((HttpWebResponse)webException.Response).StatusCode != HttpStatusCode.MethodNotAllowed)
                    {
                        throw ex;
                    }
                } else
                {
                    throw ex;
                }
            }

            try
            {
                WebRequest request = WebRequest.Create($"{folderUrl}/JURIDICO");
                request.Credentials = credentials;
                request.PreAuthenticate = true;
                request.Method = WebRequestMethods.Http.MkCol;
                WebResponse responseNet = request.GetResponse();
                responseNet.Close();
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(System.Net.WebException))
                {
                    var webException = (WebException)ex;
                    if (((HttpWebResponse)webException.Response).StatusCode != HttpStatusCode.MethodNotAllowed)
                    {
                        throw ex;
                    }
                } else
                {
                    throw ex;
                }
            }
        }

        private static void CreateFolderCRM(string folderName, Guid solicitudId, IOrganizationService service)
        {
            Guid parentId = ObtenerCRMSharePointDocumentLocationByUrl("opportunity", service);

            Entity entSharePointDocumentLocation = new Entity("sharepointdocumentlocation");
            entSharePointDocumentLocation.Attributes["name"] = "SharePoint Document Location for opportunity";
            entSharePointDocumentLocation.Attributes["description"] = "SharePoint Document Location creado para almacenar documentos relacionados a la solicitud";
            entSharePointDocumentLocation.Attributes["relativeurl"] = folderName;
            entSharePointDocumentLocation.Attributes["regardingobjectid"] = new EntityReference("opportunity", solicitudId);
            entSharePointDocumentLocation.Attributes["parentsiteorlocation"] = new EntityReference("sharepointdocumentlocation", parentId);
            service.Create(entSharePointDocumentLocation);
        }

        private static Guid ObtenerCRMSharePointDocumentLocationByUrl(string relativeUrl, IOrganizationService service)
        {
            Guid SPLocationGuid = Guid.Empty;
            ColumnSet cols = new ColumnSet();
            cols.AddColumns(new string[] { "sharepointdocumentlocationid" });

            ConditionExpression condition = new ConditionExpression();
            condition.AttributeName = "relativeurl";
            condition.Operator = ConditionOperator.Equal;
            condition.Values.Add(relativeUrl);

            FilterExpression filter = new FilterExpression();
            filter.FilterOperator = LogicalOperator.And;
            filter.Conditions.AddRange(condition);

            QueryExpression query = new QueryExpression();
            query.EntityName = "sharepointdocumentlocation";
            query.ColumnSet = cols;
            query.Criteria = filter;

            var result = service.RetrieveMultiple(query);

            if(result.Entities.Any())
            {
                var entity = result.Entities.FirstOrDefault();
                if(entity.Attributes.Contains("sharepointdocumentlocationid"))
                {
                    SPLocationGuid = new Guid(entity.AtributoColleccion("sharepointdocumentlocationid", TipoAtributos.STRING).ToString());
                }
            }

            return SPLocationGuid;
        }

        private static Entity ObtenerCRMSharePointDocumentLocationByRegardingId(Guid regardingobjectid, IOrganizationService service)
        {
            Entity sharepointDocumentLocation = null;
            ColumnSet cols = new ColumnSet();
            cols.AddColumns(new string[] { "sharepointdocumentlocationid", "relativeurl", "regardingobjectid" });

            ConditionExpression condition = new ConditionExpression();
            condition.AttributeName = "regardingobjectid";
            condition.Operator = ConditionOperator.Equal;
            condition.Values.Add(regardingobjectid);

            FilterExpression filter = new FilterExpression();
            filter.FilterOperator = LogicalOperator.And;
            filter.Conditions.AddRange(condition);

            QueryExpression query = new QueryExpression();
            query.EntityName = "sharepointdocumentlocation";
            query.ColumnSet = cols;
            query.Criteria = filter;

            var result = service.RetrieveMultiple(query);

            if (result.Entities.Any())
            {
                var entity = result.Entities.FirstOrDefault();
                if (entity.Attributes.Contains("sharepointdocumentlocationid"))
                {
                    sharepointDocumentLocation = entity;
                }
            }

            return sharepointDocumentLocation;
        }
        #endregion

        private static void SendMail(Guid solictudId, string folio, Entity ultimaBitacora, IOrganizationService service, out string errors)
        {
            errors = "";
            try
            {
                string emailSender = ConfigurationManager.AppSettings["EmailSender"];
                string emailReceiver = ConfigurationManager.AppSettings["EmailReceiver"];

                var solicitudEntity = service.Retrieve("opportunity", solictudId, new ColumnSet(new string[] { "fib_numcredito", "customerid", "ownerid", "createdon" }));
                var solicitudCreation = solicitudEntity.GetAttributeValue<DateTime>("createdon");

                var templateContent = "";
                var subject = "";
                bool segundoEnvio = false;
                bool enviarCorreo = false;

                if(solicitudEntity.GetAttributeValue<EntityReference>("customerid").LogicalName == "account")
                {
                    enviarCorreo = true;
                } else
                {
                    String fetchAvales = "<fetch mapping=\"logical\" count=\"1\" version=\"1.0\">" +
                                "<entity name=\"fib_aval\">" +
                                    "<attribute name=\"createdon\" />" +
                                    "<attribute name=\"fib_tipo\" />" +
                                    "<filter>" +
                                        "<condition attribute=\"fib_tipo\" operator=\"eq\" value=\"1\" />" +
                                        $"<condition attribute=\"fib_opportunityid\" operator=\"eq\" value=\"{solictudId}\" />" +
                                    "</filter>" +
                                "</entity>" +
                            "</fetch>";

                    var resultsAvales = service.RetrieveMultiple(new FetchExpression(fetchAvales));

                    enviarCorreo = resultsAvales.Entities.Any();
                }

                if (!enviarCorreo)
                {
                    return;
                }

                var ultimoEnvio = ultimaBitacora.GetAttributeValue<string>("fib_tipoenvio");

                if (!string.IsNullOrEmpty(ultimoEnvio) && ultimoEnvio == TIPO_BITACORA_SEGUNDO_ENVIO)
                {
                    segundoEnvio = true;
                }

                if(segundoEnvio)
                {
                    templateContent = System.IO.File.ReadAllText(HostingEnvironment.MapPath(@"~/App_Data/NOTIFICACION_ACEPTACION.html"));
                    subject = $"Actualización de solicitud para dictamen: {folio}";
                } else
                {
                    templateContent = System.IO.File.ReadAllText(HostingEnvironment.MapPath(@"~/App_Data/NOTIFICACION_ADMISION.html"));
                    subject = $"Nueva solicitud para dictamen: {folio}";
                }

                var senderUserId = ExisteUsuarioSistema(emailSender, service);
                var mailboxId = FindEntityId("mailbox", emailSender, "emailaddress", service);

                var owner = solicitudEntity.GetAttributeValue<EntityReference>("ownerid").Name;
                var customer = solicitudEntity.GetAttributeValue<EntityReference>("customerid").Name;
                 
                templateContent = templateContent.Replace("{!solicitante}", customer);
                templateContent = templateContent.Replace("{!owner}", owner);
                templateContent = templateContent.Replace("{!numCredito}", solicitudEntity.GetAttributeValue<string>("fib_numcredito"));

                Entity emailCreate = new Entity("email");
                emailCreate["subject"] = subject;
                emailCreate["description"] = templateContent;

                Entity activityTo = new Entity();
                Entity activityFrom = new Entity();

                activityTo = new Entity("activityparty");
                activityTo["addressused"] = emailReceiver;
                Entity[] toList = { activityTo };
                emailCreate["to"] = toList;

                activityFrom = new Entity("activityparty");
                activityFrom["partyid"] = new EntityReference("systemuser", senderUserId);
                Entity[] fromList = { activityFrom };
                emailCreate["from"] = fromList;

                emailCreate["regardingobjectid"] = new EntityReference("opportunity", solictudId);
                emailCreate["sendermailboxid"] = new EntityReference("mailbox", mailboxId);

                CreateRequest reqCreate = new CreateRequest();

                reqCreate.Target = emailCreate;
                CreateResponse createResponse = (CreateResponse)service.Execute(reqCreate);

                if (createResponse.id != Guid.Empty)
                {
                    SendEmailRequest sendEmailreq = new SendEmailRequest
                    {
                        EmailId = createResponse.id,
                        TrackingToken = "",
                        IssueSend = true
                    };

                    SendEmailResponse sendEmailresp = (SendEmailResponse)service.Execute(sendEmailreq);
                }
            }
            catch (Exception ex)
            {
                errors += "Error al enviar notificación por correo: " + ex.Message;
            }
        }
    }
}
