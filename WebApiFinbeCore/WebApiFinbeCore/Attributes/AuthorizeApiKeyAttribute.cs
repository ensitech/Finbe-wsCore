using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Net.Http;
using WebApiFinbeCore.Models;
using System.Configuration;

namespace WebApiFinbeCore.Attributes
{
    public class AuthorizeApiKeyAttribute : ActionFilterAttribute,IActionFilter
    {
        public override void OnActionExecuting(HttpActionContext filterContext)
        {
            if (filterContext.Request.Headers.Authorization == null)
            {
                filterContext.Response = filterContext.Request.CreateResponse(HttpStatusCode.Forbidden, "No se encuentra el Api Key");
                return;
            }
            string apiKey = filterContext.Request.Headers.Authorization.ToString();
            string[] apikeys = ConfigurationManager.AppSettings["apikeys"] != null ? ConfigurationManager.AppSettings["apikeys"].ToString().Split(',') : null;
            //VALIDAR API KEY
            if(apikeys != null && apikeys.Length > 0)
            {
                if(apikeys.Where(ap => ap.Equals(apiKey)).Any())
                {
                    return;
                }
            }
            filterContext.Response = filterContext.Request.CreateResponse(HttpStatusCode.Forbidden, "Invalid authorization token");
            return;
        }
    }
}