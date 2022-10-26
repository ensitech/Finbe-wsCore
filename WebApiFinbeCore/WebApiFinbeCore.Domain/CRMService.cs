using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace WebApiFinbeCore.Domain
{
    public class CRMService
    {
        public static IOrganizationService createService()
        {
            string url = ConfigurationManager.AppSettings["_organizationUrlCRM"]; //"https://appscrm13.bepensa.com:444/FinancieraDes/XRMServices/2011/Organization.svc";
            string user = ConfigurationManager.AppSettings["_userCRM"];//wicaamaly";
            string domain = ConfigurationManager.AppSettings["_domainCRM"]; //"bepensa";
            string password = ConfigurationManager.AppSettings["_passwordCRM"]; //"b3p3ns4*18";
            return createService(url, user, domain, password);
        }

        private static IOrganizationService createService(string url, string user, string domain, string password)
        {
            OrganizationServiceProxy serviceProxy = null;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            IServiceConfiguration<IOrganizationService> config = ServiceConfigurationFactory.CreateConfiguration<IOrganizationService>(new Uri(url));
            switch (config.AuthenticationType)
            {
                case AuthenticationProviderType.Federation:
                    ClientCredentials clientCredentials = new ClientCredentials();
                    clientCredentials.UserName.UserName = domain + "\\" + user;
                    clientCredentials.UserName.Password = password;
                    serviceProxy = new OrganizationServiceProxy(config, clientCredentials);
                    break;
                case AuthenticationProviderType.ActiveDirectory:
                    ClientCredentials credentials = new ClientCredentials();
                    credentials.Windows.ClientCredential = new NetworkCredential(user, password, domain);
                    serviceProxy = new OrganizationServiceProxy(new Uri(url), null, credentials, null);
                    break;
            }

            if (serviceProxy.IsAuthenticated)
                Console.WriteLine(serviceProxy.IsAuthenticated);

            return serviceProxy;
        }
    }
}
