// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRestApiClient.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient
{
    using System.Threading.Tasks;
    
    using Collector.Common.RestContracts;
    using Collector.Common.RestContracts.Interfaces;

    public interface IRestApiClient
    {
        /// <summary>
        /// Invokes the action asynchronously for the specified request. Throws exception if the call is unsuccessful.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        /// The requested data.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown if request is null.</exception>
        /// <exception cref="Exceptions.RestClientCallException">Thrown if response is not OK or contains RestError.</exception>
        Task CallAsync<TResourceIdentifier>(RequestBase<TResourceIdentifier> request) 
            where TResourceIdentifier : class, IResourceIdentifier;

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
        /// <exception cref="Exceptions.RestClientCallException">Thrown if response is not OK or contains RestError.</exception>
        Task<TResponse> CallAsync<TResourceIdentifier, TResponse>(RequestBase<TResourceIdentifier, TResponse> request) 
            where TResourceIdentifier : class, IResourceIdentifier
            where TResponse : class;
    }
}