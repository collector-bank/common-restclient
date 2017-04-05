// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiClientBuilder.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient
{
    using Collector.Common.RestClient.Implementation;
    using Collector.Common.RestClient.Interfaces;

    using RestSharp.Authenticators;

    using Serilog;

    public class ApiClientBuilder
    {
        private string _baseUrl;

        private ILogger _logger;

        private IAuthenticator _authenticator;

        public ApiClientBuilder UseLogging(ILogger logger)
        {
            _logger = logger;
            return this;
        }

        public ApiClientBuilder UseBaseUrl(string baseUrl)
        {
            _baseUrl = baseUrl;
            return this;
        }


        public ApiClientBuilder UseAuthorizationHeaderFactory(IAuthorizationHeaderFactory authorizationHeaderFactory)
        {
            _authenticator = new RestSharpAuthenticator(authorizationHeaderFactory);
            return this;
        }

        public IRestApiClient Build()
        {
            var wrapper = new RestSharpClientWrapper(_authenticator, _baseUrl);

            var restSharpClient = new RestSharpApiRequestApiClient(wrapper);

            return new RestApiClient(restSharpClient, _logger);
        }
    }
}