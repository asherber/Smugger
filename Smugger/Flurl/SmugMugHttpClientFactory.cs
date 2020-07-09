using Flurl.Http;
using Flurl.Http.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TinyOAuth1;

namespace Smugger.Flurl
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
                cli.WithHeader("Accept", "application/json");
                cli.Settings.HttpClientFactory = this;
                cli.Settings.BeforeCall = DoBeforeCall;
                cli.Settings.AfterCall = DoAfterCall;
                cli.Settings.OnError = DoOnError;
            });
        }

        private void DoOnError(HttpCall call)
        {
            Trace.WriteLine($"---ERROR: {call.Exception.Message}");
            call.ExceptionHandled = true;
            throw new HttpRequestException(call.Exception.Message, call.Exception);
        }

        private static void DoBeforeCall(HttpCall call)
        {
            var msg = $"{call.Request.Method} {call.FlurlRequest.Url}";
            if (call.RequestBody != null)
                msg += $", {call.RequestBody}";

            Trace.WriteLine(msg);
        }

        private static void DoAfterCall(HttpCall call)
        {
            var status = call.Response.StatusCode;
            var msg = $"---{(int)status} {status} ({call.Duration:ss'.'fff})";
            Trace.WriteLine(msg);
        }

    }
}
