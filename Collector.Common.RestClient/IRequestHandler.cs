// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRequestHandler.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient
{
    using System.Threading.Tasks;

    using Collector.Common.RestContracts;
    using Collector.Common.RestContracts.Interfaces;

    internal interface IRequestHandler
    {
        Task<TResponse> CallAsync<TResourceIdentifier, TResponse>(RequestBase<TResourceIdentifier, TResponse> request)
            where TResourceIdentifier : class, IResourceIdentifier
            where TResponse : class;

        Task CallAsync<TResourceIdentifier>(RequestBase<TResourceIdentifier> request)
            where TResourceIdentifier : class, IResourceIdentifier;
    }
}