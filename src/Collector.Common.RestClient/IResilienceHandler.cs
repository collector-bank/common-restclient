namespace Collector.Common.RestClient
{
    using System;
    using System.Threading.Tasks;

    using Collector.Common.RestContracts;
    using Collector.Common.RestContracts.Interfaces;

    public interface IResilienceHandler
    {
        Task ExecuteAsync<TResourceIdentifier>(RequestBase<TResourceIdentifier> request, Func<Task> func)
            where TResourceIdentifier : class, IResourceIdentifier;

        Task<TResponse> ExecuteAsync<TResourceIdentifier, TResponse>(RequestBase<TResourceIdentifier, TResponse> request, Func<Task<TResponse>> func)
            where TResourceIdentifier : class, IResourceIdentifier
            where TResponse : class;
    }
}
