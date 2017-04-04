// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestSharpClientWrapper_Fake.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Unittests.Fakes
{
    using System;
    using System.Net;

    using Collector.Common.RestContracts;

    using Newtonsoft.Json;

    using RestSharp;
    using RestSharp.Authenticators;

    internal class RestSharpClientWrapper_Fake : Interfaces.IRestSharpClientWrapper
    {
        public IRestRequest LastRequest { get; private set; }

        public HttpStatusCode ExpectedResponseStatusCode { get; set; } = HttpStatusCode.OK;

        public Error ExpectedError { get; set; }

        public IAuthenticator Authenticator { get; set; }

        public IRestResponse Execute(IRestRequest request)
        {
            return ExecuteRequest(request);
        }

        public void ExecuteAsync(IRestRequest request, Action<IRestResponse> callback)
        {
            var response = ExecuteRequest(request);
            
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