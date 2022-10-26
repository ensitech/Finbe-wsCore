using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using WebApiFinbeCore.Model;
using WebApiFinbeCore.Domain;
using System.Web.Http.Cors;
using WebApiFinbeCore.Attributes;

namespace WebApiFinbeCore.Controllers
{
    public class InventoryController : BaseController
    {
        /// <summary>
        /// Obtener el estatus del credito
        /// </summary>
        /// <param name="numeroDeCliente">Número de Cliente</param>
        /// <param name="tipoDispLinea">Tipo Linea</param>
        /// <param name="fechaFinal">Fecha Final (yyyy-mm-dd)</param>
        /// <param name="fechaInicio">Fecha de Inicio (yyyy-mm-dd)</param>
        /// <param name="tipoRango">Tipo de Rango</param>
        /// <returns></returns>
        /// <response code="200">Ok. Devuelve el estatus de los creditos</response>
        /// <response code="500">InternalServerError. Devuelve el mensaje de la excepción</response>
        [HttpGet]
        [Route("InventarioCredito")]
        [ResponseType(typeof(List<EstatusCredito>))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage EstatusCredito([FromUri] string numeroDeCliente,[FromUri]string tipoDispLinea = "",[FromUri] string fechaInicio = "",[FromUri] string fechaFinal = "",int tipoRango = 0)
        {
            try
            {
                var estatusCredito = InventoryService.ObtenerEstatusDeCredito(numeroDeCliente, tipoDispLinea,fechaInicio,fechaFinal,tipoRango);
                return Request.CreateResponse(HttpStatusCode.OK, estatusCredito);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Información del Número de Serie Vehicular
        /// </summary>
        /// <param name="vin">Numero de Serie o VIN</param>
        /// <returns></returns>
        /// <response code="200">Ok. Devuelve la informacion del VIN</response>
        /// <response code="500">InternalServerError. Devuelve el mensaje de la excepción</response>
        [HttpGet]
        [Route("InformacionVin")]
        [ResponseType(typeof(InformacionVin))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage InformacionVin([FromUri] string vin)
        {
            try
            {
                var informacion = InventoryService.GetInformacionVin(vin);
                return Request.CreateResponse(HttpStatusCode.OK, informacion);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
