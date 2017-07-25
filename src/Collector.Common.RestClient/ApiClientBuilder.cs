namespace Collector.Common.RestClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Collector.Common.RestClient.Authorization;
    using Collector.Common.RestClient.Exceptions;
    using Collector.Common.RestClient.RestSharpClient;

    using Serilog;

    public class ApiClientBuilder
    {
        internal readonly IDictionary<string, Uri> BaseUris = new Dictionary<string, Uri>();
        internal readonly IDictionary<string, IAuthorizationConfiguration> Authenticators = new Dictionary<string, IAuthorizationConfiguration>();
        internal readonly IDictionary<string, TimeSpan> Timeouts = new Dictionary<string, TimeSpan>();

        private ILogger _logger;
        private Func<string> _contextFunc;

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
        /// Configure the IRestApiClient using app config values for the specified RestContract key
        /// </summary>
        /// <param name="contractKey">The key which identifies requests for contracts</param>
        public ApiClientBuilder ConfigureContractKeyFromAppSettings(string contractKey)
        {
            if (string.IsNullOrEmpty(contractKey))
                throw new ArgumentNullException(nameof(contractKey));

            if (BaseUris.ContainsKey(contractKey))
                throw new RestClientConfigurationException($"{contractKey} has already been configured.");

            BaseUris.Add(contractKey, new Uri(ConfigReader.GetAndEnsureValueFromAppSettingsKey($"{contractKey}.BaseUrl")));

            var authentication = ConfigReader.GetValueFromAppSettingsKey(contractKey, "Authentication");

            if (!string.IsNullOrEmpty(authentication))
            {
                if (authentication.ToLower() == "oauth2")
                    Authenticators.Add(contractKey, new Oauth2AuthorizationConfiguration(contractKey));
                else
                    throw new RestClientConfigurationException($"Authentication method '{authentication}' is not supported.");
            }

            var timeout = ConfigReader.GetTimeSpanValueFromAppSettingsKey(contractKey, "Timeout");
            if (timeout.HasValue)
                Timeouts[contractKey] = timeout.Value;

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
                throw new RestClientConfigurationException("Please configure atleast one base uri");
            }

            var authorizationHeaderFactories = Authenticators.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.CreateFactory(_logger));

            var wrapper = new RestSharpClientWrapper(BaseUris, authorizationHeaderFactories, Timeouts, _logger);

            var requestHandler = new RestSharpRequestHandler(wrapper);

            return new RestApiClient(requestHandler, _contextFunc);
        }
    }
}