// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRequestHandler.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Interfaces
{
    using System.Threading.Tasks;

    using Collector.Common.RestContracts;
    using Collector.Common.RestContracts.Interfaces;

    public interface IRequestHandler
    {
        Task<TResponse> CallAsync<TResourceIdentifier, TResponse>(RequestBase<TResourceIdentifier, TResponse> request)
            where TResourceIdentifier : class, IResourceIdentifier;

        Task CallAsync<TResourceIdentifier>(RequestBase<TResourceIdentifier> request)
            where TResourceIdentifier : class, IResourceIdentifier;
    }
}