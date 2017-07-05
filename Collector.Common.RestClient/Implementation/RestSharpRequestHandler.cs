// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestSharpRequestHandler.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Implementation
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    
    using Collector.Common.RestClient.Exceptions;
    using Collector.Common.RestClient.Interfaces;
    using Collector.Common.RestContracts;
    using Collector.Common.RestContracts.Interfaces;

    using Newtonsoft.Json;

    using RestSharp;

    /// <summary>
    /// </summary>
    internal class RestSharpRequestHandler : IRequestHandler
    {
        private readonly IRestSharpClientWrapper _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestSharpRequestHandler"/> class.
        /// </summary>
        /// <param name="client">The rest client wrapper.</param>
        internal RestSharpRequestHandler(IRestSharpClientWrapper client)
        {
            _client = client;
        }

        /// <summary>
        /// Invokes the action asynchronously for the specified request. Throws exception if the call is unsuccessful.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        /// The requested data.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown if request is null.</exception>
        /// <exception cref="RequestValidationException">Thrown if request is invalid.</exception>
        /// <exception cref="RestApiException">Thrown if response is not OK or contains RestError.</exception>
        public async Task CallAsync<TResourceIdentifier>(RequestBase<TResourceIdentifier> request)
            where TResourceIdentifier : class, IResourceIdentifier
        {
            var restRequest = CreateRestRequest(request);

            await GetResponseAsync<object>(restRequest, request.GetConfigurationKey());
        }

        /// <summary>
        /// Gets the data asynchronously for the specified request. Throws exception if the call is unsuccessful.
        /// </summary>
        /// <typeparam name="TResourceIdentifier">The resource identifier</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The request.</param>
        /// <returns>
        /// The requested data.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown if request is null.</exception>
        /// <exception cref="RequestValidationException">Thrown if request is invalid.</exception>
        /// <exception cref="RestApiException">Thrown if response is not OK or contains RestError.</exception>
        public async Task<TResponse> CallAsync<TResourceIdentifier, TResponse>(RequestBase<TResourceIdentifier, TResponse> request)
            where TResourceIdentifier : class, IResourceIdentifier
        {
            var restRequest = CreateRestRequest(request);

            return await GetResponseAsync<TResponse>(restRequest, request.GetConfigurationKey());
        }

        private static void AddParametersFromRequest(IRestRequest restRequest, object request)
        {
            if (restRequest.Method != Method.GET)
            {
                restRequest.AddJsonBody(request);
                return;
            }

            var parameters = request.GetType()
                                    .GetProperties()
                                    .Where(p => p.GetValue(request, null) != null)
                                    .Where(p => !typeof(IResourceIdentifier).IsAssignableFrom(p.PropertyType))
                                    .Select(p => new { p.Name, Value = p.GetValue(request, null) })
                                    .ToList();

            if (!parameters.Any())
                return;

            foreach (var parameter in parameters)
                restRequest.AddParameter(parameter.Name, parameter.Value, "application/json", ParameterType.GetOrPost);
        }

        private static RestRequest CreateRestRequest<TResourceIdentifier>(RequestBase<TResourceIdentifier> request) where TResourceIdentifier : class, IResourceIdentifier
        {
            var restRequest = new RestRequest(request.GetResourceIdentifier().Uri, GetMethod(request.GetHttpMethod()));

            AddParametersFromRequest(restRequest, request);

            return restRequest;
        }

        private static Method GetMethod(HttpMethod method)
        {
            return (Method)Enum.Parse(typeof(Method), method.ToString());
        }

        private Task<TResponse> GetResponseAsync<TResponse>(IRestRequest restRequest, string contractIdentifier)
        {
            var taskCompletionSource = new TaskCompletionSource<TResponse>();

            _client.ExecuteAsync(
                restRequest,
                contractIdentifier,
                response =>
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject<Response<TResponse>>(response.Content);

                        if (result.Error != null)
                        {
                            taskCompletionSource.SetException(new RestApiException(response.StatusCode, result.Error));
                        }
                        else if (!IsSuccessStatusCode(response))
                        {
                            taskCompletionSource.SetException(new RestApiException(
                                httpStatusCode: response.StatusCode,
                                message: "No response body recieved"));
                        }
                        else
                        {
                            taskCompletionSource.SetResult(result.Data);
                        }
                    }
                    catch (Exception ex)
                    {
                        taskCompletionSource.SetException(new RestApiException(
                             httpStatusCode: response.StatusCode,
                             restError: new Error
                             {
                                 Message = "Failed to deserialize message.",
                                 Code = response.StatusCode.ToString(),
                                 Errors = new[]
                                          {
                                              new ErrorInfo
                                              {
                                                Reason   = ex.GetType().Name,
                                                Message = ex.Message
                                              } 
                                          }
                             }));
                    }
                });

            return taskCompletionSource.Task;
        }

        public bool IsSuccessStatusCode(IRestResponse response) => (response.StatusCode >= HttpStatusCode.OK) && (response.StatusCode <= (HttpStatusCode)299);
    }
}