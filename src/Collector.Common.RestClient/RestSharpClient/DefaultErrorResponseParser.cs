namespace Collector.Common.RestClient.RestSharpClient
{
    using Collector.Common.RestContracts;
    using Collector.Common.RestContracts.Interfaces;

    using Newtonsoft.Json;

    internal class DefaultErrorResponseParser : IErrorResponseParser
    {
        public Error ParseError(string content) => JsonConvert.DeserializeObject<Response<object>>(content)?.Error;
    }
}