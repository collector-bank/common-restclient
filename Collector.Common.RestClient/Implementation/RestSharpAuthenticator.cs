// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestSharpAuthenticator.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Implementation
{
    using System.Linq;

    using Collector.Common.RestClient.Authorization;
    using Collector.Common.RestClient.Interfaces;

    using RestSharp;
    using RestSharp.Authenticators;

    internal class RestSharpAuthenticator : IAuthenticator
    {
        private readonly IAuthorizationHeaderFactory _authorizationHeaderFactory;

        public RestSharpAuthenticator(IAuthorizationHeaderFactory authorizationHeaderFactory)
        {
            _authorizationHeaderFactory = authorizationHeaderFactory;
        }

        public void Authenticate(IRestClient client, IRestRequest request)
        {
            var body = GetBody(request);

            var httpRequestData = new RestAuthorizeRequestData(body, client.BuildUri(request), request.Method);

            var authorizationHeaderValue = _authorizationHeaderFactory.Get(httpRequestData);

            request.AddParameter("Authorization", authorizationHeaderValue, ParameterType.HttpHeader);
        }

        private static string GetBody(IRestRequest request)
        {
            var bodyParameter = request.Parameters.FirstOrDefault(p => p.Type == ParameterType.RequestBody);
            if (bodyParameter == null)
                return string.Empty;

            return bodyParameter.Value as string;
        }
    }
}