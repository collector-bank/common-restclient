// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestSharpClientWrapper_Fake.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.UnitTests.Fakes
{
    using System;
    using System.Net;

    using Collector.Common.RestClient.RestSharpClient;
    using Collector.Common.RestContracts;
    using Collector.Common.RestContracts.Interfaces;

    using global::RestSharp;
    using global::RestSharp.Authenticators;

    using Newtonsoft.Json;

    using Serilog;

    internal class RestSharpClientWrapper_Fake : IRestSharpClientWrapper
    {
        public IRestRequest LastRequest { get; private set; }

        public HttpStatusCode ExpectedResponseStatusCode { get; set; } = HttpStatusCode.OK;

        public Error ExpectedError { get; set; }

        public IAuthenticator Authenticator { get; set; }

        public IRestResponse Execute(IRestRequest request)
        {
            return ExecuteRequest(request);
        }

        public void ExecuteAsync(IRestRequest restRequest, IRequest request, Action<IRestResponse> callback)
        {
            var response = ExecuteRequest(restRequest);
            
            callback(response);
        }

        private IRestResponse ExecuteRequest(IRestRequest request)
        {
            LastRequest = request;
            var response = ExpectedError != null
                ? new Response<object>("1.0", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), error: ExpectedError)
                : new Response<object>("1.0", Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), data: null);

            return new RestResponse { StatusCode = ExpectedResponseStatusCode, Content = JsonConvert.SerializeObject(response) };
        }
    }
}