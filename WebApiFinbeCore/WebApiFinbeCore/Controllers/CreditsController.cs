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
    public class CreditsController : BaseController
    {
        [HttpGet]
        [Route("CreditosActivos")]
        [ResponseType(typeof(CreditoRolesList))]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage CreditosActivos([FromUri] string rfc)
        {
            try
            {
                var creditos = InventoryService.CreditosActivos(rfc);
                return Request.CreateResponse(HttpStatusCode.OK, creditos);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
