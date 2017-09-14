namespace Collector.Common.RestClient.RestSharpClient
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using Collector.Common.RestClient.Exceptions;
    using Collector.Common.RestContracts;
    using Collector.Common.RestContracts.Interfaces;

    using RestSharp;

    internal class RestSharpRequestHandler : IRequestHandler
    {
        private readonly IRestSharpClientWrapper _client;

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
        /// <exception cref="RestClientCallException">Thrown if response is not OK or contains RestError.</exception>
        public async Task CallAsync<TResourceIdentifier>(RequestBase<TResourceIdentifier> request)
            where TResourceIdentifier : class, IResourceIdentifier
        {
            var restRequest = CreateRestRequest(request);

            await GetResponseAsync<object>(restRequest, request).ConfigureAwait(false);
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
        /// <exception cref="RestClientCallException">Thrown if response is not OK or contains RestError.</exception>
        public async Task<TResponse> CallAsync<TResourceIdentifier, TResponse>(RequestBase<TResourceIdentifier, TResponse> request)
            where TResourceIdentifier : class, IResourceIdentifier
            where TResponse : class
        {
            var restRequest = CreateRestRequest(request);

            return await GetResponseAsync<TResponse>(restRequest, request).ConfigureAwait(false);
        }

        private static void AddParametersFromRequest(IRestRequest restRequest, object request)
        {
            if (restRequest.Method != Method.GET && restRequest.Method != Method.DELETE)
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
            var restRequest = new RestRequest(request.GetResourceIdentifier().Uri, GetMethod(request.GetHttpMethod()))
                              {
                                  JsonSerializer = new NewtonsoftJsonSerializer()
                              };

            AddParametersFromRequest(restRequest, request);

            return restRequest;
        }

        private static Method GetMethod(HttpMethod method)
        {
            return (Method)Enum.Parse(typeof(Method), method.ToString());
        }

        private static bool IsSuccessStatusCode(IRestResponse response) => (response.StatusCode >= HttpStatusCode.OK) && (response.StatusCode <= (HttpStatusCode)299);

        private static RestClientCallException ParseError(IRestResponse response, IRequest request)
        {
            var parser = request as IErrorResponseParser ?? new DefaultErrorResponseParser();
            var error = parser.ParseError(response.Content);
            return error?.Message != null
                       ? new RestClientCallException(httpStatusCode: response.StatusCode, restError: error)
                       : new RestClientCallException(httpStatusCode: response.StatusCode, message: $"Rest request not successful, {response.StatusCode}");
        }

        private static TResponse ParseSuccess<TResponse>(IRestResponse response, IRequest request) where TResponse : class
        {
            if (typeof(TResponse) == typeof(Stream))
                return new MemoryStream(response.RawBytes) as TResponse;

            var parser = request as ISuccessfulResponseParser<TResponse> ?? new DefaultSuccessfulResponseParser<TResponse>();

            return parser.ParseResponse(response.Content);
        }

        private Task<TResponse> GetResponseAsync<TResponse>(IRestRequest restRequest, IRequest request) where TResponse : class
        {
            var taskCompletionSource = new TaskCompletionSource<TResponse>();

            _client.ExecuteAsync(
                restRequest,
                request,
                response => Callback(response, taskCompletionSource, request));

            return taskCompletionSource.Task;
        }

        private void Callback<TResponse>(IRestResponse response, TaskCompletionSource<TResponse> taskCompletionSource, IRequest request)
            where TResponse : class
        {
            if (response.StatusCode == 0)
            {
                taskCompletionSource.SetException(new RestClientCallException(response.StatusCode, "No response from server"));
                return;
            }

            try
            {
                if (IsSuccessStatusCode(response))
                    taskCompletionSource.SetResult(ParseSuccess<TResponse>(response, request));
                else
                    taskCompletionSource.SetException(ParseError(response, request));
            }
            catch (Exception ex)
            {
                taskCompletionSource.SetException(
                    new RestClientCallException(
                        httpStatusCode: response.StatusCode,
                        message: "An error occurred while reading the response.",
                        innerException: ex));
            }
        }
    }
}