using Flurl;
using Flurl.Http;
using System;
using System.Net.Http;
using TinyOAuth1;

namespace SmugMug.NET
{
    public class SmugMugAuthorizer
    {
        private OAuthCredentials _credentials;
        private ITinyOAuth _oauthClient;
        private LoginType _loginType;

        public SmugMugAuthorizer(LoginType loginType, OAuthCredentials credentials)
        {
            _loginType = loginType;
            _credentials = credentials;
            _oauthClient = new TinyOAuth(new TinyOAuthConfig()
            {
                ConsumerKey = credentials.ConsumerKey,
                ConsumerSecret = credentials.ConsumerSecret
            });
        }

        public void AddAuth(HttpRequestMessage request)
        {
            if (_loginType == LoginType.Anonymous)
            {
                var url = request.RequestUri.AbsoluteUri;
                url.SetQueryParam("APIKey", _credentials.ConsumerKey);
                request.RequestUri = new Uri(url);
            }
            else
            {
                var header = _oauthClient.GetAuthorizationHeader(
                    _credentials.AccessToken,
                    _credentials.AccessTokenSecret,
                    request.RequestUri.AbsoluteUri,
                    request.Method
                );
                request.Headers.Authorization = header;
            }
        }
    }
}
