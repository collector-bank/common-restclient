// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestApiClient.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using Collector.Common.Library.Collections;
    using Collector.Common.Library.Validation;
    using Collector.Common.RestClient.Authorization;
    using Collector.Common.RestClient.Exceptions;
    using Collector.Common.RestClient.Interfaces;
    using Collector.Common.RestContracts;
    using Collector.Common.RestContracts.Interfaces;

    using Newtonsoft.Json;

    using RestSharp;

    /// <summary>
    /// </summary>
    public class RestApiClient : IRestApiClient
    {
        private const string NULL_RESPONSE = "NULL_RESPONSE";

        private readonly IRestSharpClientWrapper _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestApiClient"/> class.
        /// </summary>
        public RestApiClient()
            : this(new RestSharpClientWrapper())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestApiClient"/> class.
        /// </summary>
        /// <param name="client">The rest client wrapper.</param>
        internal RestApiClient(IRestSharpClientWrapper client)
        {
            _client = client;
        }

        public IAuthorizationHeaderFactory AuthorizationHeaderFactory
        {
            set
            {
                _client.Authenticator = new RestSharpAuthenticator(value);
            }
        }

        /// <summary>
        /// Invokes the action asynchronously for the specified request. Throws exception if the call is unsuccessful.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        /// The requested data.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown if request is null.</exception>
        /// <exception cref="ValidationException">Thrown if request is invalid.</exception>
        /// <exception cref="RestApiException">Thrown if response is not OK or contains RestError.</exception>
        public async Task CallAsync<TResourceIdentifier>(RequestBase<TResourceIdentifier> request)
            where TResourceIdentifier : ResourceIdentifier
        {
            EnsureRequestObjectIsValid(request);

            var restRequest = CreateRestRequest(request);

            await GetResponseAsync<object>(restRequest);
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
        /// <exception cref="ValidationException">Thrown if request is invalid.</exception>
        /// <exception cref="RestApiException">Thrown if response is not OK or contains RestError.</exception>
        public async Task<TResponse> CallAsync<TResourceIdentifier, TResponse>(RequestBase<TResourceIdentifier, TResponse> request)
            where TResourceIdentifier : ResourceIdentifier
        {
            EnsureRequestObjectIsValid(request);

            var restRequest = CreateRestRequest(request);

            return await GetResponseAsync<TResponse>(restRequest);
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
                                    .ToFixed();

            if (!parameters.Any())
                return;

            foreach (var parameter in parameters)
                restRequest.AddParameter(parameter.Name, parameter.Value, "application/json", ParameterType.GetOrPost);
        }

        private static RestRequest CreateRestRequest<TResourceIdentifier>(RequestBase<TResourceIdentifier> request) where TResourceIdentifier : ResourceIdentifier
        {
            var restRequest = new RestRequest(request.GetResourceIdentifier().Uri, GetMethod(request.GetHttpMethod()));

            AddParametersFromRequest(restRequest, request);

            return restRequest;
        }

        private static void EnsureRequestObjectIsValid(object request)
        {
            var exception = AnnotationValidator.Validate(request, AnnotationValidator.ValidationBehaviour.Deep);
            if (exception != null)
                throw exception;
        }

        private static Method GetMethod(HttpMethod method)
        {
            return (Method)Enum.Parse(typeof(Method), method.ToString());
        }

        private Task<TResponse> GetResponseAsync<TResponse>(IRestRequest restRequest)
        {
            var taskCompletionSource = new TaskCompletionSource<TResponse>();

            _client.ExecuteAsync(
                restRequest,
                response =>
                {
                    if (response.StatusCode < HttpStatusCode.OK || response.StatusCode >= (HttpStatusCode)299)
                    {
                        taskCompletionSource.SetException(new RestApiException(message: "Failed with code " + response.StatusCode, errorCode: NULL_RESPONSE));
                        return;
                    }

                    var result = JsonConvert.DeserializeObject<Response<TResponse>>(response.Content);

                    if (result.Error != null)
                        taskCompletionSource.SetException(new RestApiException(result.Error));
                    else
                        taskCompletionSource.SetResult(result.Data);
                });

            return taskCompletionSource.Task;
        }
    }
}