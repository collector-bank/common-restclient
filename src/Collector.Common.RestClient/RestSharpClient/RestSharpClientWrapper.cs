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
    using Newtonsoft.Json.Linq;

    using RestSharp;

    using Serilog;

    internal sealed class RestSharpClientWrapper : IRestSharpClientWrapper
    {
        internal readonly ConcurrentDictionary<string, IRestClient> RestClients = new ConcurrentDictionary<string, IRestClient>();
        private readonly IDictionary<string, Uri> _baseUrlMappings;
        private readonly IDictionary<string, IAuthorizationHeaderFactory> _authorizationHeaderFactories;
        private readonly IDictionary<string, TimeSpan> _timeouts;
        private readonly ILogger _logger;

        public RestSharpClientWrapper(
            IDictionary<string, Uri> baseUrlMappings, 
            IDictionary<string, IAuthorizationHeaderFactory> authorizationHeaderFactories,
            IDictionary<string, TimeSpan> timeouts,
            ILogger logger)
        {
            _baseUrlMappings = baseUrlMappings;
            _authorizationHeaderFactories = authorizationHeaderFactories;
            _timeouts = timeouts;
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

            if (_authorizationHeaderFactories.ContainsKey(contractKey))
                client.Timeout = (int)_timeouts[contractKey].TotalMilliseconds;

            RestClients.TryAdd(contractKey, client);
        }

        public void ExecuteAsync(IRestRequest restRequest, IRequest request, Action<IRestResponse> callback)
        {
            var contractKey = request.GetConfigurationKey();
            if (!RestClients.ContainsKey(contractKey))
            {
                InitRestClient(contractKey);
            }

            var restClient = RestClients[contractKey];


            TryLogRequest(restRequest, request, restClient);

            var stopwatch = Stopwatch.StartNew();
            restClient.ExecuteAsync(
                restRequest,
                response =>
                {
                    stopwatch.Stop();
                    TryLogResponse(restRequest, stopwatch, response, request);

                    callback(response);
                });
        }

        private void TryLogRequest(IRestRequest restRequest, IRequest request, IRestClient restClient)
        {
            try
            {
                var restClientLogProperty = new
                                            {
                                                RequestContent = restRequest.Method == Method.GET || restRequest.Method == Method.DELETE
                                                                     ? string.Empty
                                                                     : JsonConvert.SerializeObject(request, Formatting.Indented),
                                                HttpRequestUrl = restClient.BuildUri(restRequest).ToString(),
                                                HttpRequestType = restRequest.Method
                                            };

                _logger?.ForContext("RestClient", restClientLogProperty, destructureObjects: true)
                       ?.Information("Rest request sent to {ConfigurationKey}", request.GetConfigurationKey());
            }
            catch (Exception e)
            {
                _logger?.Warning(e, "There was a problem logging the rest request");
            }
        }

        private void TryLogResponse(IRestRequest restRequest, Stopwatch stopwatch, IRestResponse response, IRequest request)
        {
            try
            {
                var isJsonResponse = response.ContentType?.ToLower().Contains("application/json") ?? false;
                var restClientLogProperty = new
                                            {
                                                HttpRequestUrl = response.ResponseUri,
                                                HttpRequestType = restRequest.Method,
                                                StatusCode = (int)response.StatusCode,
                                                MediaType = response.ContentType,
                                                ResponseTimeMilliseconds = (int)stopwatch.ElapsedMilliseconds,
                                                RawResponseBody = isJsonResponse ? response.Content : "Response not in json format",
                                                ResponseBody = isJsonResponse ? GetFormatedResponseContent(response) : "Response not in json format"
                                            };

                _logger?.ForContext("RestClient", restClientLogProperty, destructureObjects: true)
                       ?.Information("Rest response recieved from {ConfigurationKey}", request.GetConfigurationKey());
            }
            catch (Exception e)
            {
                _logger?.Warning(e, "There was a problem logging the rest response");
            }
        }

        private string GetFormatedResponseContent(IRestResponse response)
        {
            try
            {
                var jObject = (JObject)JsonConvert.DeserializeObject(response.Content);

                return JsonConvert.SerializeObject(jObject, Formatting.Indented);
            }
            catch
            {
                return "Could not format the raw response body";
            }
        }
    }
}