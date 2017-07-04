// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestApiClient.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Implementation
{
    using System.Linq;
    using System.Threading.Tasks;

    using Collector.Common.RestClient.Exceptions;
    using Collector.Common.RestClient.Interfaces;
    using Collector.Common.RestContracts;
    using Collector.Common.RestContracts.Interfaces;

    using Serilog;

    internal class RestApiClient : IRestApiClient
    {
        private readonly IRequestHandler _requestHandler;
        private readonly ILogger _logger;

        internal RestApiClient(IRequestHandler requestHandler, ILogger logger)
        {
            _requestHandler = requestHandler;
            _logger = logger?.ForContext<RestApiClient>();
        }

        public Task CallAsync<TResourceIdentifier>(RequestBase<TResourceIdentifier> request) where TResourceIdentifier : class, IResourceIdentifier
        {
            LogRequest(request);
            EnsureRequestObjectIsValid(request);
            return _requestHandler.CallAsync(request);
        }

        public async Task<TResponse> CallAsync<TResourceIdentifier, TResponse>(RequestBase<TResourceIdentifier, TResponse> request) where TResourceIdentifier : class, IResourceIdentifier
        {
            LogRequest(request);
            EnsureRequestObjectIsValid(request);
            var response = await _requestHandler.CallAsync(request);

            _logger?.Information("CallAsync response {@ResponseType}", response.GetType());
            return response;
        }

        private void LogRequest<TResourceIdentifier>(RequestBase<TResourceIdentifier> request) where TResourceIdentifier : class, IResourceIdentifier
        {
            _logger?.Information("CallAsync request type {Type} identifier {@Identifier}", request.GetType(), request.GetResourceIdentifier());
        }

        private void EnsureRequestObjectIsValid(IRequest request)
        {
            var errorInfos = request.GetValidationErrors().ToList();
            if (errorInfos != null && errorInfos.Any())
                throw new ValidationException(errorInfos);
        }
    }
}