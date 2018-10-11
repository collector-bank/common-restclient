﻿namespace Collector.Common.RestClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Collector.Common.RestClient.Authorization;
    using Collector.Common.RestClient.Configuration;
    using Collector.Common.RestClient.Exceptions;
    using Collector.Common.RestClient.RestSharpClient;

    using Serilog;

    public class ApiClientBuilderBase
    {
        internal readonly IDictionary<string, Uri> BaseUris = new Dictionary<string, Uri>();
        internal readonly IDictionary<string, Func<IConfigReader>> Configurations = new Dictionary<string, Func<IConfigReader>>();
        internal readonly IDictionary<string, IAuthorizationConfiguration> Authenticators = new Dictionary<string, IAuthorizationConfiguration>();
        internal readonly IDictionary<string, Func<IConfigReader, IAuthorizationConfiguration>> AuthenticationMethods = new Dictionary<string, Func<IConfigReader, IAuthorizationConfiguration>>()
                                                                                                                        {
                                                                                                                            ["oauth2"] = configReader => new Oauth2AuthorizationConfiguration(configReader)
                                                                                                                        };
        internal readonly IDictionary<string, TimeSpan> Timeouts = new Dictionary<string, TimeSpan>();

        private ILogger _logger;
        private Func<string> _contextFunc;
        private Func<string, string> _configurationKeyDecorator = s => s;

        /// <summary>
        /// Configure the IRestApiClient by RestContract key
        /// </summary>
        /// <param name="contractKey">The key which identifies requests for contracts</param>
        /// <param name="baseUrl">Api base url</param>
        /// <param name="authorizationConfiguration">(optional) Authorization header factory creating the Authorization header for the request</param>
        public ApiClientBuilderBase ConfigureContractByKey(string contractKey, string baseUrl, IAuthorizationConfiguration authorizationConfiguration = null)
        {
            if (string.IsNullOrEmpty(contractKey))
                throw new ArgumentNullException(nameof(contractKey));

            if (BaseUris.ContainsKey(contractKey))
                throw new RestClientConfigurationException($"{contractKey} has already been configured.");

            BaseUris.Add(contractKey, new Uri(baseUrl));

            if (authorizationConfiguration != null)
            {
                Authenticators.Add(contractKey, authorizationConfiguration);
            }

            return this;
        }

        public ApiClientBuilderBase WithContextFunction(Func<string> contextFunc)
        {
            _contextFunc = contextFunc;
            return this;
        }

        public ApiClientBuilderBase WithConfiguration(string contractKey, Func<IConfigReader> configBuilder)
        {
            if (string.IsNullOrEmpty(contractKey))
                throw new ArgumentNullException(nameof(contractKey));

            if (configBuilder == null)
                throw new ArgumentNullException(nameof(configBuilder));

            Configurations[contractKey] = configBuilder;

            return this;
        }

        public ApiClientBuilderBase WithConfigurationKeyDecorator(Func<string, string> configurationKeyDecorator)
        {
            _configurationKeyDecorator = configurationKeyDecorator;
            return this;
        }

        public ApiClientBuilderBase RegisterAuthenticator(string authenticationMethod, Func<IConfigReader, IAuthorizationConfiguration> authorizationConfigurationBuilder)
        {
            if (authorizationConfigurationBuilder == null)
                throw new ArgumentNullException(nameof(authorizationConfigurationBuilder));

            AuthenticationMethods[authenticationMethod.ToLower()] = authorizationConfigurationBuilder;

            return this;
        }

        /// <summary>
        /// Configures serilog for all requests made by the IRestApiClient that's beeing built
        /// </summary>
        /// <param name="logger">Configured ILogger</param>
        public ApiClientBuilderBase WithLogger(ILogger logger)
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
            var authorizationHeaderFactories = Authenticators.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.CreateFactory(_logger));

            foreach (var contractKey in Configurations.Keys)
            {
                ConfigureContractKey(contractKey, Configurations[contractKey]());
            }

            if (!BaseUris.Any())
            {
                throw new RestClientConfigurationException("Please configure atleast one base uri");
            }

            var wrapper = new RestSharpClientWrapper(BaseUris, authorizationHeaderFactories, Timeouts, _configurationKeyDecorator, _logger);

            var requestHandler = new RestSharpRequestHandler(wrapper);

            return new RestApiClient(requestHandler, _contextFunc);
        }

        protected void ConfigureContractKey(string contractKey, IConfigReader configReader)
        {
            BaseUris.Add(contractKey, new Uri(configReader.GetAndEnsureString("BaseUrl", onlyFromSubSection: true)));

            var authentication = configReader.GetString("Authentication");

            if (!string.IsNullOrEmpty(authentication))
            {
                if(AuthenticationMethods.ContainsKey(authentication.ToLower()))
                    Authenticators.Add(contractKey, AuthenticationMethods[authentication.ToLower()](configReader));
                else
                    throw new RestClientConfigurationException($"Authentication method '{authentication}' is not supported.");
            }

            var timeout = configReader.GetTimeSpan("Timeout");
            if (timeout.HasValue)
                Timeouts[contractKey] = timeout.Value;
        }
    }
}