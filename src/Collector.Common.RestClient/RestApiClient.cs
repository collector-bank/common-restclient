namespace Collector.Common.RestClient
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using Collector.Common.RestClient.Exceptions;
    using Collector.Common.RestContracts;
    using Collector.Common.RestContracts.Interfaces;

    internal class RestApiClient : IRestApiClient
    {
        private readonly IRequestHandler _requestHandler;
        private readonly Func<string> _contextFunc;
        private readonly IResilienceHandler _resilienceHandler;

        public RestApiClient(IRequestHandler requestHandler, Func<string> contextFunc = null, IResilienceHandler resilienceHandler = null)
        {
            _requestHandler = requestHandler;
            _contextFunc = contextFunc;
            _resilienceHandler = resilienceHandler;
        }

        public Task CallAsync<TResourceIdentifier>(RequestBase<TResourceIdentifier> request) 
            where TResourceIdentifier : class, IResourceIdentifier
        {
            request.Context = request.Context ?? _contextFunc?.Invoke();
            EnsureRequestObjectIsValid(request);

            if (_resilienceHandler == null)
            {
                return _requestHandler.CallAsync(request);
            }

            return _resilienceHandler.ExecuteAsync(request, () => _requestHandler.CallAsync(request));
        }

        public async Task<TResponse> CallAsync<TResourceIdentifier, TResponse>(RequestBase<TResourceIdentifier, TResponse> request) 
            where TResourceIdentifier : class, IResourceIdentifier
            where TResponse : class
        {
            request.Context = request.Context ?? _contextFunc?.Invoke();
            EnsureRequestObjectIsValid(request);

            if (_resilienceHandler == null)
            {
                return await _requestHandler.CallAsync(request).ConfigureAwait(false);
            }

            return await _resilienceHandler.ExecuteAsync(request, () => _requestHandler.CallAsync(request)).ConfigureAwait(false);
        }

        private void EnsureRequestObjectIsValid(IRequest request)
        {
            var errorInfos = request.GetValidationErrors().ToList();
            if (errorInfos != null && errorInfos.Any())
                throw new RestClientCallException(HttpStatusCode.BadRequest, errorInfos);
        }
    }
}