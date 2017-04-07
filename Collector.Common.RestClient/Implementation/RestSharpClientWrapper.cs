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
        internal readonly IDictionary<string, IRestClient> RestClients = new Dictionary<string, IRestClient>();

        public RestSharpClientWrapper(IDictionary<string, string> baseUrlMappings, IDictionary<string, IAuthorizationHeaderFactory> authorizationHeaderFactories)
        {
            InitRestClients(baseUrlMappings, authorizationHeaderFactories);
        }

        private void InitRestClients(IDictionary<string, string> baseUrlMappings, IDictionary<string, IAuthorizationHeaderFactory> authorizationHeaderFactories)
        {
            foreach (var baseUrlMapping in baseUrlMappings)
            {
                IAuthenticator authenticator = null;

                if (authorizationHeaderFactories.ContainsKey(baseUrlMapping.Key))
                {
                    authenticator = new RestSharpAuthenticator(authorizationHeaderFactories[baseUrlMapping.Key]);
                }

                var client = new RestClient
                {
                    BaseUrl = new Uri(baseUrlMapping.Value),
                    Authenticator = authenticator
                };

                RestClients.Add(baseUrlMapping.Key, client);
            }
        }

        public void ExecuteAsync(IRestRequest request, string contractIdentifier, Action<IRestResponse> callback)
        {
            IRestClient client;

            if (!RestClients.TryGetValue(contractIdentifier, out client))
            {
                throw new ArgumentOutOfRangeException($"No mapping found for contract identifier : {contractIdentifier}");
            }

            client.ExecuteAsync(request, callback);
        }
    }
}