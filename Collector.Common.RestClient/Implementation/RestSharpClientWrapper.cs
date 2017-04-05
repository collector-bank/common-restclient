// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestSharpClientWrapper.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Implementation
{
    using System;

    using Collector.Common.RestClient.Interfaces;

    using RestSharp;
    using RestSharp.Authenticators;

    internal sealed class RestSharpClientWrapper : IRestSharpClientWrapper
    {
        private readonly IRestClient _restClient;

        public RestSharpClientWrapper(IAuthenticator authenticator, string baseUrl)
        {
            _restClient = new RestClient(baseUrl) { Authenticator = authenticator };
        }

        public void ExecuteAsync(IRestRequest request, Action<IRestResponse> callback)
        {
            _restClient.ExecuteAsync(request, callback);
        }
    }
}