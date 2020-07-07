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

namespace SmugMug.NET.Flurl
{
    // Ideas from https://github.com/metasys-server/redirect-handler
    public class HttpClientRedirectHandler : DelegatingHandler
    {
        private OAuthCredentials _credentials;
        private ITinyOAuth _oauthClient;
        private static IList<HttpStatusCode> _redirectCodes = new List<HttpStatusCode>() 
        { 
            HttpStatusCode.TemporaryRedirect, HttpStatusCode.MovedPermanently 
        };


        public bool EnforceHostNameMatching { get; set; }


        public HttpClientRedirectHandler(OAuthCredentials credentials, ITinyOAuth oauthClient) 
            : base(new HttpClientHandler() { AllowAutoRedirect = false })
        {
            _credentials = credentials;
            _oauthClient = oauthClient;
            EnforceHostNameMatching = true;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            if (_redirectCodes.Contains(response.StatusCode) && request.Headers.Contains("Authorization"))
            {
                var newLocation = response.Headers.Location;

                if (newLocation?.IsAbsoluteUri == false)
                {
                    var root = Url.GetRoot(request.RequestUri.AbsoluteUri);
                    newLocation = new Uri(Url.Combine(root, newLocation.ToString()));
                }

                if (newLocation == null
                    || (EnforceHostNameMatching && request.RequestUri.Host != newLocation.Host))
                {
                    return response;
                }                

                request.RequestUri = newLocation;
                
                var authHeader = _oauthClient.GetAuthorizationHeader(_credentials.AccessToken, _credentials.AccessTokenSecret,
                        newLocation.AbsoluteUri, request.Method);
                request.Headers.Authorization = authHeader;

                return await base.SendAsync(request, cancellationToken);
            }

            return response;
        }
    }
}
