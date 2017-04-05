// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestApiClient.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient
{
    using System.Threading.Tasks;

    using Collector.Common.Library.Validation;
    using Collector.Common.RestClient.Interfaces;
    using Collector.Common.RestContracts;
    using Collector.Common.RestContracts.Interfaces;

    using Serilog;

    internal class RestApiClient : IRestApiClient
    {
        private readonly IRequestApiClient _requestApiClient;
        private readonly ILogger _logger;

        internal RestApiClient(IRequestApiClient requestApiClient, ILogger logger)
        {
            _requestApiClient = requestApiClient;
            _logger = logger?.ForContext<RestApiClient>();
        }

        public Task CallAsync<TResourceIdentifier>(RequestBase<TResourceIdentifier> request) where TResourceIdentifier : class, IResourceIdentifier
        {
            LogRequest(request);
            EnsureRequestObjectIsValid(request);
            return _requestApiClient.CallAsync(request);
        }

        public async Task<TResponse> CallAsync<TResourceIdentifier, TResponse>(RequestBase<TResourceIdentifier, TResponse> request) where TResourceIdentifier : class, IResourceIdentifier
        {
            LogRequest(request);
            EnsureRequestObjectIsValid(request);
            var response = await _requestApiClient.CallAsync(request);

            _logger?.Information("CallAsync response {@ResponseType}", response.GetType());
            return response;
        }

        private void LogRequest<TResourceIdentifier>(RequestBase<TResourceIdentifier> request) where TResourceIdentifier : class, IResourceIdentifier
        {
            _logger?.Information("CallAsync request type {Type} identifier {@Identifier}", request.GetType(), request.GetResourceIdentifier());
        }

        private void EnsureRequestObjectIsValid(object request)
        {
            var exception = AnnotationValidator.Validate(request, AnnotationValidator.ValidationBehaviour.Deep);
            if (exception != null)
                throw exception;
        }
    }
}