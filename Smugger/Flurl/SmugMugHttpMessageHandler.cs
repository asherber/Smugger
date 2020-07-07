using Flurl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TinyOAuth1;

namespace Smugger.Flurl
{
    // Ideas for redirect from https://github.com/metasys-server/redirect-handler
    public class SmugMugHttpMessageHandler : DelegatingHandler
    {
        private SmugMugAuthorizer _authorizer;
        private static IList<HttpStatusCode> _redirectCodes = new List<HttpStatusCode>() 
        { 
            HttpStatusCode.TemporaryRedirect, 
            HttpStatusCode.MovedPermanently 
        };


        public SmugMugHttpMessageHandler(SmugMugAuthorizer authorizer) 
            : base(new HttpClientHandler() { AllowAutoRedirect = false })
        {
            _authorizer = authorizer;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _authorizer.AddAuth(request);
            var response = await base.SendAsync(request, cancellationToken);

            if (_redirectCodes.Contains(response.StatusCode))
            {
                var newLocation = response.Headers.Location;

                if (newLocation?.IsAbsoluteUri == false)
                {
                    var root = Url.GetRoot(request.RequestUri.AbsoluteUri);
                    newLocation = new Uri(Url.Combine(root, newLocation.ToString()));
                }

                if (newLocation == null
                    || request.RequestUri.Host != newLocation.Host)
                {
                    return response;
                }                

                request.RequestUri = newLocation;
                return await this.SendAsync(request, cancellationToken);
            }

            return response;
        }
    }
}
