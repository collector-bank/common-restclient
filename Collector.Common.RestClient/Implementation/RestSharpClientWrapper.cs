// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestSharpClientWrapper.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Implementation
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using Collector.Common.RestClient.Exceptions;
    using Collector.Common.RestClient.Interfaces;

    using RestSharp;
    using RestSharp.Authenticators;

    internal sealed class RestSharpClientWrapper : IRestSharpClientWrapper
    {
        private readonly IDictionary<string, string> _baseUrlMappings;
        private readonly IDictionary<string, IAuthorizationHeaderFactory> _authorizationHeaderFactories;

        internal readonly ConcurrentDictionary<string, IRestClient> RestClients = new ConcurrentDictionary<string, IRestClient>();

        public RestSharpClientWrapper(IDictionary<string, string> baseUrlMappings, IDictionary<string, IAuthorizationHeaderFactory> authorizationHeaderFactories)
        {
            _baseUrlMappings = baseUrlMappings;
            _authorizationHeaderFactories = authorizationHeaderFactories;
        }

        internal void InitRestClient(string contractKey)
        {
            if (!_baseUrlMappings.ContainsKey(contractKey))
            {
                throw new BuildException($"No mapping found for contract identifier : {contractKey}");
            }

            IAuthenticator authenticator = null;

            if (_authorizationHeaderFactories.ContainsKey(contractKey))
            {
                authenticator = new RestSharpAuthenticator(_authorizationHeaderFactories[contractKey]);
            }

            var baseUrl = _baseUrlMappings[contractKey];

            var client = new RestClient
                         {
                             BaseUrl = new Uri(baseUrl),
                             Authenticator = authenticator
                         };

            RestClients.TryAdd(contractKey, client);
        }

        public void ExecuteAsync(IRestRequest request, string contractKey, Action<IRestResponse> callback)
        {
            if (!RestClients.ContainsKey(contractKey))
            {
                InitRestClient(contractKey);
            }

            var restClient = RestClients[contractKey];

            restClient.ExecuteAsync(request, callback);
        }
    }
}