namespace Collector.Common.RestClient.RestSharpClient
{
    using System;

    using Collector.Common.RestContracts.Interfaces;

    using RestSharp;

    internal interface IRestSharpClientWrapper
    {
        void ExecuteAsync(IRestRequest restRequest, IRequest request, Action<IRestResponse> callback);
    }
}