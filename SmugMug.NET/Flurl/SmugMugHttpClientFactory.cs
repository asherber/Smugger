using Flurl.Http;
using Flurl.Http.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TinyOAuth1;

namespace SmugMug.NET.Flurl
{
    public class SmugMugHttpClientFactory : DefaultHttpClientFactory
    {
        private SmugMugAuthorizer _authorizer;

        public SmugMugHttpClientFactory(SmugMugAuthorizer authorizer)
        {
            _authorizer = authorizer;
        }

        public override HttpMessageHandler CreateMessageHandler()
        {
            return new SmugMugHttpMessageHandler(_authorizer);
        }

        public void ConfigureFlurlClient(string url)
        {
            FlurlHttp.ConfigureClient(url, cli =>
            {
                cli.Headers.Add("Accept", "application/json");
                cli.Settings.HttpClientFactory = this;
            });
        }
    }
}
