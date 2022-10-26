using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using WebApiFinbeCore.Domain;
using WebApiFinbeCore.Model;

namespace WebApiFinbeCore.Controllers
{
    public class NominaController : BaseController
    {
        [HttpGet]
        [Route("Poliza")]
        [ResponseType(typeof(PolizaResponse))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage Poliza([FromUri] string cia, int formato, int poliza, int anio, int mes)
        {
            try
            {
                var response = NominaService.ObtenerPoliza(cia, formato, poliza, anio, mes);
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
