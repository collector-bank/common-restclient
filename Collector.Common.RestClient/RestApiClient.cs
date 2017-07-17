// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestApiClient.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

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
        private readonly Func<string> _contexFunc;

        internal RestApiClient(IRequestHandler requestHandler, Func<string> contexFunc)
        {
            _requestHandler = requestHandler;
            _contexFunc = contexFunc;
        }

        public Task CallAsync<TResourceIdentifier>(RequestBase<TResourceIdentifier> request) 
            where TResourceIdentifier : class, IResourceIdentifier
        {
            request.Context = request.Context ?? _contexFunc?.Invoke();
            EnsureRequestObjectIsValid(request);
            return _requestHandler.CallAsync(request);
        }

        public async Task<TResponse> CallAsync<TResourceIdentifier, TResponse>(RequestBase<TResourceIdentifier, TResponse> request) 
            where TResourceIdentifier : class, IResourceIdentifier
            where TResponse : class
        {
            request.Context = request.Context ?? _contexFunc?.Invoke();
            EnsureRequestObjectIsValid(request);
            var response = await _requestHandler.CallAsync(request);
            
            return response;
        }

        private void EnsureRequestObjectIsValid(IRequest request)
        {
            var errorInfos = request.GetValidationErrors().ToList();
            if (errorInfos != null && errorInfos.Any())
                throw new RestClientCallException(HttpStatusCode.BadRequest, errorInfos);
        }
    }
}