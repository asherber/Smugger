using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TinyOAuth1;

namespace Smugger.Samples
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

        private static async Task<OAuthCredentials> GenerateOAuthAccessTokenAsync(string apiKey, string secret)
        {
            var config = new TinyOAuthConfig()
            {
                AccessTokenUrl = "https://api.smugmug.com/services/oauth/1.0a/getAccessToken",
                AuthorizeTokenUrl = "https://api.smugmug.com/services/oauth/1.0a/authorize",
                RequestTokenUrl = "https://api.smugmug.com/services/oauth/1.0a/getRequestToken",
                ConsumerKey = apiKey,
                ConsumerSecret = secret
            };
            var client = new TinyOAuth(config);


            // Request token
            var requestTokenInfo = await client.GetRequestTokenAsync();

            // Authorization
            var authorizationUrl = client.GetAuthorizationUrl(requestTokenInfo.RequestToken);
            System.Diagnostics.Process.Start(authorizationUrl);

            Console.WriteLine("Enter the six-digit code: ");
            string verifier = Console.ReadLine();

            // Access Token
            var accessTokenInfo = await client.GetAccessTokenAsync(
                requestTokenInfo.RequestToken,
                requestTokenInfo.RequestTokenSecret,
                verifier
            );

            return new OAuthCredentials(apiKey, secret, accessTokenInfo.AccessToken, accessTokenInfo.AccessTokenSecret);
        }
    }
}
