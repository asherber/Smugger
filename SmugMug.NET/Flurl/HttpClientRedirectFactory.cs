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
    public class HttpClientRedirectFactory : DefaultHttpClientFactory
    {
        private OAuthCredentials _credentials;
        private ITinyOAuth _oauthClient;

        public HttpClientRedirectFactory(OAuthCredentials credentials, ITinyOAuth oauthClient)
        {
            _credentials = credentials;
            _oauthClient = oauthClient;
        }

        public override HttpMessageHandler CreateMessageHandler()
        {
            return new HttpClientRedirectHandler(_credentials, _oauthClient);
        }

    }
}
