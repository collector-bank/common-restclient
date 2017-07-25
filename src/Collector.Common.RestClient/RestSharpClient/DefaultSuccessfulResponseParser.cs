namespace Collector.Common.RestClient.RestSharpClient
{
    using Collector.Common.RestContracts;
    using Collector.Common.RestContracts.Interfaces;

    using Newtonsoft.Json;

    internal class DefaultSuccessfulResponseParser<TResponse> : ISuccessfulResponseParser<TResponse>
    {
        public TResponse ParseResponse(string content) => JsonConvert.DeserializeObject<Response<TResponse>>(content).Data;
    }
}