// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiClientBuilder.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Collector.Common.RestClient.Authorization;
    using Collector.Common.RestClient.Exceptions;
    using Collector.Common.RestClient.Implementation;
    using Collector.Common.RestClient.Interfaces;

    using Serilog;

    public class ApiClientBuilder
    {
        internal readonly IDictionary<string, string> BaseUris = new Dictionary<string, string>();
        internal readonly IDictionary<string, IAuthorizationHeaderFactory> Authenticators = new Dictionary<string, IAuthorizationHeaderFactory>();

        private ILogger _logger;
        private Func<string> _contextFunc;

        /// <summary>
        /// Configure the IRestApiClient by RestContract key
        /// </summary>
        /// <param name="contractKey">The key which identifies requests for contracts</param>
        /// <param name="baseUrl">Api base url</param>
        /// <param name="authorizationHeaderFactory">(optional) Authorization header factory creating the Authorization header for the request</param>
        public ApiClientBuilder ConfigureContractByKey(string contractKey, string baseUrl, IAuthorizationHeaderFactory authorizationHeaderFactory = null)
        {
            if (string.IsNullOrEmpty(contractKey))
                throw new ArgumentNullException(nameof(contractKey));

            if (BaseUris.ContainsKey(contractKey))
                throw new BuildException($"{contractKey} has already been configured.");

            BaseUris.Add(contractKey, baseUrl);

            if (authorizationHeaderFactory != null)
            {
                Authenticators.Add(contractKey, authorizationHeaderFactory);
            }

            return this;
        }


        /// <summary>
        /// Configure the IRestApiClient using app config values for the specified RestContract key
        /// </summary>
        /// <param name="contractKey">The key which identifies requests for contracts</param>
        public ApiClientBuilder ConfigureContractKeyFromAppSettings(string contractKey)
        {
            if (string.IsNullOrEmpty(contractKey))
                throw new ArgumentNullException(nameof(contractKey));

            if (BaseUris.ContainsKey(contractKey))
                throw new BuildException($"{contractKey} has already been configured.");

            BaseUris.Add(contractKey, ConfigReader.GetAndEnsureValueFromAppSettingsKey($"{contractKey}.BaseUrl"));

            var authentication = ConfigReader.GetValueFromAppSettingsKey(contractKey, "Authentication");

            if (!string.IsNullOrEmpty(authentication))
            {
                if (authentication.ToLower() == "oauth2")
                    Authenticators.Add(contractKey, new Oauth2AuthorizationHeaderFactory(contractKey));
                else
                    throw new NotSupportedException($"Authentication method '{authentication}' is not supported.");
            }

            return this;
        }

        public ApiClientBuilder WithContextFunction(Func<string> contextFunc)
        {
            _contextFunc = contextFunc;
            return this;
        }

        /// <summary>
        /// Configures serilog for all requests made by the IRestApiClient that's beeing built
        /// </summary>
        /// <param name="logger">Configured ILogger</param>
        public ApiClientBuilder ConfigureLogging(ILogger logger)
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
            if (!BaseUris.Any())
            {
                throw new BuildException("Please configure atleast one base uri");
            }

            var wrapper = new RestSharpClientWrapper(BaseUris, Authenticators, _logger);

            var requestHandler = new RestSharpRequestHandler(wrapper);

            return new RestApiClient(requestHandler, _contextFunc);
        }
    }
}