using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiFinbeCore.Attributes;

namespace WebApiFinbeCore.Controllers
{
    [AuthorizeApiKey]
    public class BaseController : ApiController
    {
        
    }
}
