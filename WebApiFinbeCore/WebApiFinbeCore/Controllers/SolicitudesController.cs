using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using WebApiFinbeCore.Attributes;
using WebApiFinbeCore.Domain;
using WebApiFinbeCore.Model;
using WebApiFinbeCore.Models;

namespace WebApiFinbeCore.Controllers
{
    public class SolicitudesController : BaseController
    {
        /// <summary>
        /// Crear una solicitud
        /// </summary>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Solicitud")]
        [ResponseType(typeof(ApiResponse<SolicitudResponse>))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage CrearSolicitud([FromBody] Solicitud solicitud)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var response = SolicitudesService.ProcesarSolicitud(solicitud);

                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    var errors = string.Join(",", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage + ":" + e.Exception?.Message).ToArray());
                    SolicitudesService.GeneraBitacoraModelo(solicitud, errors);
                    return Request.CreateResponse(HttpStatusCode.BadRequest,errors);
                }
                
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Endpoint para asignar documentos a una solicitud de crédito
        /// </summary>
        /// <param name="documentoSolicitud"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Solicitud/Documento")]
        [ResponseType(typeof(DocumentoSolicitudResponse))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage CrearDocumentoSolicitud([FromBody] DocumentoSolicitudRequest documentoSolicitud)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var response = SolicitudesService.ProcesarDocumentoSolicitud(documentoSolicitud);

                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    var errors = string.Join(",", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage + ":" + e.Exception?.Message).ToArray());
                    //SolicitudesService.GeneraBitacoraModelo(solicitud, errors);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, errors);
                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Endpoint para la actualización de solicitudes de crédito
        /// </summary>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("Solicitud")]
        [ResponseType(typeof(SolicitudResponse))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage ActualizarSolicitud([FromBody] SolicitudActualizacion solicitud)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var response = SolicitudesService.ProcesarActualizacionSolicitud(solicitud);

                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    var errors = string.Join(",", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage + ":" + e.Exception?.Message).ToArray());
                    //SolicitudesService.GeneraBitacoraModelo(solicitud, errors);
                    return Request.CreateResponse(HttpStatusCode.BadRequest, errors);
                }

            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
