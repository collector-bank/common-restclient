namespace Collector.Common.RestClient.RestSharpClient
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;

    using Collector.Common.RestClient.Authorization;
    using Collector.Common.RestClient.Exceptions;
    using Collector.Common.RestContracts.Interfaces;

    using Newtonsoft.Json;

    using RestSharp;

    using Serilog;

    internal sealed class RestSharpClientWrapper : IRestSharpClientWrapper
    {
        internal readonly ConcurrentDictionary<string, IRestClient> RestClients = new ConcurrentDictionary<string, IRestClient>();
        private readonly IDictionary<string, Uri> _baseUrlMappings;
        private readonly IDictionary<string, IAuthorizationHeaderFactory> _authorizationHeaderFactories;
        private readonly IDictionary<string, TimeSpan> _timeouts;
        private readonly Func<string, string> _configurationKeyDecorator;
        private readonly ILogger _logger;

        public RestSharpClientWrapper(
            IDictionary<string, Uri> baseUrlMappings, 
            IDictionary<string, IAuthorizationHeaderFactory> authorizationHeaderFactories,
            IDictionary<string, TimeSpan> timeouts,
            Func<string, string> configurationKeyDecorator,
            ILogger logger)
        {
            _baseUrlMappings = baseUrlMappings;
            _authorizationHeaderFactories = authorizationHeaderFactories;
            _timeouts = timeouts;
            _configurationKeyDecorator = configurationKeyDecorator;
            _logger = logger?.ForContext(GetType());
        }

        public void InitRestClient(string contractKey)
        {
            if (!_baseUrlMappings.ContainsKey(contractKey))
                throw new RestClientConfigurationException($"No mapping found for contract identifier : {contractKey}");

            var client = new RestClient
                         {
                             BaseUrl = _baseUrlMappings[contractKey],
                         };

            if (_authorizationHeaderFactories.ContainsKey(contractKey))
                client.Authenticator = new RestSharpAuthenticator(_authorizationHeaderFactories[contractKey]);

            if (_timeouts.ContainsKey(contractKey))
                client.Timeout = (int)_timeouts[contractKey].TotalMilliseconds;

            RestClients.TryAdd(contractKey, client);
        }

        public void ExecuteAsync(IRestRequest restRequest, IRequest request, Action<IRestResponse> callback)
        {
            var configurationKey = _configurationKeyDecorator(request.GetConfigurationKey());
            if (!RestClients.ContainsKey(configurationKey))
            {
                InitRestClient(configurationKey);
            }

            var restClient = RestClients[configurationKey];

            TryLogRequest(restRequest, request, restClient, configurationKey);

            var stopwatch = Stopwatch.StartNew();
            restClient.ExecuteAsync(
                restRequest,
                response =>
                {
                    stopwatch.Stop();
                    TryLogResponse(restRequest, stopwatch, response, request, configurationKey);

                    callback(response);
                });
        }

        private void TryLogRequest(IRestRequest restRequest, IRequest request, IRestClient restClient, string configurationKey)
        {
            try
            {
                var restClientLogProperty = new
                                            {
                                                RequestContent = request.GetRequestContentForLogging(JsonConvert.SerializeObject(request)),
                                                HttpRequestUrl = restClient.BuildUri(restRequest).ToString(),
                                                HttpRequestType = restRequest.Method
                                            };

                _logger?.ForContext("RestClient", restClientLogProperty, destructureObjects: true)
                       ?.Information("Rest request sent to {ConfigurationKey}", configurationKey);
            }
            catch (Exception e)
            {
                _logger?.Warning(e, "There was a problem logging the rest request");
            }
        }

        private void TryLogResponse(IRestRequest restRequest, Stopwatch stopwatch, IRestResponse response, IRequest request, string configurationKey)
        {
            try
            {
                var restClientLogProperty = new
                                            {
                                                HttpRequestUrl = response.ResponseUri,
                                                HttpRequestType = restRequest.Method,
                                                StatusCode = (int)response.StatusCode,
                                                MediaType = response.ContentType,
                                                ResponseTimeMilliseconds = (int)stopwatch.ElapsedMilliseconds,
                                                ResponseContentLength = (int)response.ContentLength,
                                                RawResponseContent = request.GetRawResponseContentForLogging(response.Content, response.ContentType),
                                                ResponseContent = request.GetResponseContentForLogging(response.Content, response.ContentType)
                                            };

                _logger?.ForContext("RestClient", restClientLogProperty, destructureObjects: true)
                       ?.Information("Rest response recieved from {ConfigurationKey}", configurationKey);
            }
            catch (Exception e)
            {
                _logger?.Warning(e, "There was a problem logging the rest response");
            }
        }
    }
}