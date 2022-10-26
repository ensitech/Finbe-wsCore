using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace WebApiFinbeCore.Domain
{
    public static class Api360Service<T>
    {
        public static T Exec(string uri)
        {
            var urlApi360 = ConfigurationManager.AppSettings["api360"];
            var api360UserName = ConfigurationManager.AppSettings["api360UserName"];
            var api360Password = ConfigurationManager.AppSettings["api360Password"];

            const SslProtocols _Tls12 = (SslProtocols)0x00000C00;
            const SecurityProtocolType Tls12 = (SecurityProtocolType)_Tls12;
            ServicePointManager.SecurityProtocol = Tls12;
            ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;

            var client = new RestClient(urlApi360 + uri);
            client.Timeout = -1;
            client.Authenticator = new HttpBasicAuthenticator(api360UserName, api360Password);

            var request = new RestRequest();
            request.Method = Method.GET;
            request.RequestFormat = DataFormat.Json;
            var response = client.Execute(request);

            return JsonConvert.DeserializeObject<T>(response.Content);

        }
    }
}