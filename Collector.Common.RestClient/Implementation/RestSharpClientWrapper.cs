// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestSharpClientWrapper.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Implementation
{
    using System;
    using System.Collections.Generic;

    using Collector.Common.RestClient.Interfaces;

    using RestSharp;
    using RestSharp.Authenticators;

    internal sealed class RestSharpClientWrapper : IRestSharpClientWrapper
    { 
        private readonly IDictionary<string, string> _baseUrls;

        private readonly IDictionary<string, IAuthenticator> _authenticators;

        public RestSharpClientWrapper(IDictionary<string, string> baseUrls, IDictionary<string, IAuthenticator> authenticators)
        {
            _baseUrls = baseUrls;
            _authenticators = authenticators;
        }

        public void ExecuteAsync(IRestRequest request, string contractIdentifier, Action<IRestResponse> callback)
        {
            string baseUrl;

            if (!_baseUrls.TryGetValue(contractIdentifier, out baseUrl))
            {
                throw new ArgumentOutOfRangeException($"No mapping found for contract identifier : {contractIdentifier}");
            }

            IAuthenticator authenticator;

            _authenticators.TryGetValue(contractIdentifier, out authenticator);

            var restClient = new RestClient(baseUrl)
            {
                Authenticator = authenticator
            };

            restClient.ExecuteAsync(request, callback);
        }
    }
}