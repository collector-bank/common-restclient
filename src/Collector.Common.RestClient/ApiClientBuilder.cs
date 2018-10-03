namespace Collector.Common.RestClient
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;

    using Collector.Common.RestClient.Authorization;
    using Collector.Common.RestClient.Configuration;
    using Collector.Common.RestClient.Exceptions;
    using Collector.Common.RestClient.RestSharpClient;
    using Collector.Common.RestContracts.Interfaces;

    using Microsoft.Extensions.Configuration;

    using Serilog;

    public class ApiClientBuilder
    {
        internal readonly IDictionary<string, Uri> BaseUris = new Dictionary<string, Uri>();
        internal readonly IDictionary<string, IAuthorizationConfiguration> Authenticators = new Dictionary<string, IAuthorizationConfiguration>();
        internal readonly IDictionary<string, Func<IConfigReader, IAuthorizationConfiguration>> AuthenticationMethods = new Dictionary<string, Func<IConfigReader, IAuthorizationConfiguration>>()
                                                                                                                        {
                                                                                                                            ["oauth2"] = configReader => new Oauth2AuthorizationConfiguration(configReader)
                                                                                                                        };
        internal readonly IDictionary<string, TimeSpan> Timeouts = new Dictionary<string, TimeSpan>();

        internal readonly IDictionary<string, ISuccessfulResponseParser> _successfulResponseParsers = new Dictionary<string, ISuccessfulResponseParser>();
        internal readonly IDictionary<string, IErrorResponseParser> _errorResponseParsers = new Dictionary<string, IErrorResponseParser>();

        private ILogger _logger;
        private Func<string> _contextFunc;
        private Func<string, string> _configurationKeyDecorator = s => s;

        /// <summary>
        /// Configure the IRestApiClient by RestContract key
        /// </summary>
        /// <param name="contractKey">The key which identifies requests for contracts</param>
        /// <param name="baseUrl">Api base url</param>
        /// <param name="authorizationConfiguration">(optional) Authorization header factory creating the Authorization header for the request</param>
        public ApiClientBuilder ConfigureContractByKey(string contractKey, string baseUrl, IAuthorizationConfiguration authorizationConfiguration = null)
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

        /// <summary>
        /// Configure the IRestApiClient using app config values.
        /// </summary>
        public ApiClientBuilder ConfigureFromAppSettings()
        {
            foreach (var baseUrlSetting in ConfigurationManager.AppSettings.AllKeys.Where(k => k.StartsWith("RestClient:")).Where(k => k.EndsWith(".BaseUrl")))
            {
                var contractKey = baseUrlSetting.Split(':').Last().Split('.').First();
                ConfigureContractKey(contractKey, new AppConfigReader(contractKey));
            }

            return this;
        }

        /// <summary>
        /// Configure the IRestApiClient using a configuration section.
        /// </summary>
        public ApiClientBuilder ConfigureFromConfigSection(IConfigurationSection configurationSection)
        {
            foreach (var subSection in configurationSection.GetSection("Apis").GetChildren())
            {
                ConfigureContractKey(subSection.Key, new SectionConfigReader(configurationSection, subSection));
            }

            return this;
        }

        public ApiClientBuilder WithContextFunction(Func<string> contextFunc)
        {
            _contextFunc = contextFunc;
            return this;
        }

        public ApiClientBuilder WithConfigurationKeyDecorator(Func<string, string> configurationKeyDecorator)
        {
            _configurationKeyDecorator = configurationKeyDecorator;
            return this;
        }

        public ApiClientBuilder RegisterAuthenticator(string authenticationMethod, Func<IConfigReader, IAuthorizationConfiguration> authorizationConfigurationBuilder)
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
        public ApiClientBuilder WithLogger(ILogger logger)
        {
            _logger = logger;
            return this;
        }

        /// <summary>
        /// Register error response parser by API configuration
        /// </summary>
        /// <param name="contractKey">API configuration name to target with error response parser registration</param>
        /// <param name="parser">Implementation of <see cref="IErrorResponseParser"/></param>
        public ApiClientBuilder WithResponseParser(string contractKey, IErrorResponseParser parser)
        {
            if (string.IsNullOrWhiteSpace(contractKey))
            {
                throw new ArgumentNullException(nameof(contractKey));
            }

            _errorResponseParsers[contractKey] = parser;

            return this;
        }

        /// <summary>
        /// Register successful response parser by API configuration
        /// </summary>
        /// <param name="contractKey">API configuration name to target with successful response parser registration</param>
        /// <param name="parser">Implementation of <see cref="object"/></param>
        public ApiClientBuilder WithResponseParser(string contractKey, ISuccessfulResponseParser parser)
        {
            if (string.IsNullOrWhiteSpace(contractKey))
            {
                throw new ArgumentNullException(nameof(contractKey));
            }

            _successfulResponseParsers[contractKey] = parser;

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
                throw new RestClientConfigurationException("Please configure at least one base uri");
            }

            var authorizationHeaderFactories = Authenticators.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.CreateFactory(_logger));

            var wrapper = new RestSharpClientWrapper(BaseUris, authorizationHeaderFactories, Timeouts, _configurationKeyDecorator, _logger);

            var requestHandler = new RestSharpRequestHandler(wrapper, _successfulResponseParsers, _errorResponseParsers);

            return new RestApiClient(requestHandler, _contextFunc);
        }

        private void ConfigureContractKey(string contractKey, IConfigReader configReader)
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