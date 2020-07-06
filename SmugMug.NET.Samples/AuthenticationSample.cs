using Flurl;
using Flurl.Http;
using OAuth;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SmugMug.NET.Samples
{
    class AuthenticationSample
    {
        const string CONSUMERTOKEN = "SmugMugOAuthConsumerToken";
        const string CONSUMERSECRET = "SmugMugOAuthConsumerSecret";

        public static SmugMugClient AuthenticateUsingAnonymous()
        {
            //Access OAuth keys from App.config
            string consumerKey = null;
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var keySetting = config.AppSettings.Settings[CONSUMERTOKEN];
            if (keySetting != null)
            {
                consumerKey = keySetting.Value;
            }

            if (String.IsNullOrEmpty(consumerKey))
            {
                throw new ConfigurationErrorsException("The OAuth consumer token must be specified in App.config");
            }

            //Connect to SmugMug using Anonymous access
            SmugMugClient apiAnonymous = new SmugMugClient(consumerKey);
            return apiAnonymous;
        }

        public static async Task<SmugMugClient> AuthenticateUsingOAuth()
        {
            //Access OAuth keys from App.config
            string consumerKey = null;
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var keySetting = config.AppSettings.Settings[CONSUMERTOKEN];
            if (keySetting != null)
            {
                consumerKey = keySetting.Value;
            }
            if (String.IsNullOrEmpty(consumerKey))
            {
                throw new ConfigurationErrorsException("The OAuth consumer token must be specified in App.config");
            }

            string secret = null;
            keySetting = config.AppSettings.Settings[CONSUMERSECRET];
            if (keySetting != null)
            {
                secret = keySetting.Value;
            }
            if (String.IsNullOrEmpty(secret))
            {
                throw new ConfigurationErrorsException("The OAuth consumer token secret must be specified in App.config");
            }

            //Generate oAuthCredentials using OAuth library
            OAuthCredentials oAuthCredentials = await GenerateOAuthAccessTokenAsync(consumerKey, secret).ConfigureAwait(false);

            //Connect to SmugMug using oAuth
            SmugMugClient apiOAuth = new SmugMugClient(oAuthCredentials);
            return apiOAuth;
        }

        private static async Task<string> GetTokenAsync(OAuthRequest request, string url)
        {
            request.RequestUrl = url;
            var authHeader = request.GetAuthorizationHeader();
            var result = await url.WithHeader("Authorization", authHeader).GetStringAsync();
            return result;
        }

        private static string GetTokenByName(string tokens, string name)
        {
            var parsedTokens = Url.ParseQueryParams(tokens);
            return parsedTokens.Single(t => t.Name == name).Value.ToString();
        }

        private static async Task<OAuthCredentials> GenerateOAuthAccessTokenAsync(string apiKey, string secret)
        {
            string baseUrl = "https://api.smugmug.com";
            string requestPath = "/services/oauth/1.0a/getRequestToken";
            string authorizePath = "/services/oauth/1.0a/authorize";
            string accessPath = "/services/oauth/1.0a/getAccessToken";

            // Request token
            var request = OAuthRequest.ForRequestToken(apiKey, secret, "oob");
            var tokenResult = await GetTokenAsync(request, baseUrl.AppendPathSegment(requestPath));
            var requestToken = GetTokenByName(tokenResult, "oauth_token");
            var requestTokenSecret = GetTokenByName(tokenResult, "oauth_token_secret");

            // Authorization
            var authorizationUrl = baseUrl.AppendPathSegment(authorizePath)
                .SetQueryParams(new
                {
                    mode = "auth_req_token",
                    oauth_token = requestToken,
                    Access = "Full",
                    Permissions = "Modify"
                });
            System.Diagnostics.Process.Start(authorizationUrl);

            Console.WriteLine("Enter the six-digit code: ");
            string verifier = Console.ReadLine();

            // Access Token
            request = OAuthRequest.ForAccessToken(apiKey, secret, requestToken, requestTokenSecret, verifier);
            tokenResult = await GetTokenAsync(request, baseUrl.AppendPathSegment(accessPath));
            var accessToken = GetTokenByName(tokenResult, "oauth_token");
            var accessTokenSecret = GetTokenByName(tokenResult, "oauth_token_secret");

            return new OAuthCredentials(apiKey, secret, accessToken, accessTokenSecret);
        }
    }
}
