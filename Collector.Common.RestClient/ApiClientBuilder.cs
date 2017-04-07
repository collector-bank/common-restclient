// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiClientBuilder.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient
{
    using System.Collections.Generic;

    using Collector.Common.RestClient.Implementation;
    using Collector.Common.RestClient.Interfaces;

    using RestSharp.Authenticators;

    using Serilog;

    public class ApiClientBuilder : IApiClientBuilder
    {
        internal readonly IDictionary<string, string> BaseUris = new Dictionary<string, string>();

        internal readonly IDictionary<string, IAuthenticator> Authenticators = new Dictionary<string, IAuthenticator>();

        private ILogger _logger;
       
        public IApiClientBuilder ConfigureContractByKey(string contractKey, string baseUrl, IAuthorizationHeaderFactory authorizationHeaderFactory = null)
        {
            BaseUris.Add(contractKey, baseUrl);

            if (authorizationHeaderFactory != null)
            {
                Authenticators.Add(contractKey, new RestSharpAuthenticator(authorizationHeaderFactory));
            }

            return this;
        }

        public IApiClientBuilder ConfigureLogging(ILogger logger)
        {
            _logger = logger;
            return this;
        }

        /// <summary>
        /// Builds a configured IRestApiClient, based on currently configured configurations
        /// </summary>
        /// <returns>Fully configured IRestApiClient</returns>
        public IRestApiClient Build()
        {
            var wrapper = new RestSharpClientWrapper(BaseUris, Authenticators);

            var restSharpRequestHandler = new RestSharpRequestHandler(wrapper);

            return new RestApiClient(restSharpRequestHandler, _logger);
        }
    }
}