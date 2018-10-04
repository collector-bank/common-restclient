namespace Collector.Common.RestClient.RestSharpClient
{
    using Collector.Common.RestContracts;
    using Collector.Common.RestContracts.Interfaces;

    using Newtonsoft.Json;

    internal class DefaultSuccessfulResponseParser : ISuccessfulResponseParser
    {
        public TResponse ParseResponse<TResponse>(string content)
        {
            var settings = new JsonSerializerSettings
                           {
                                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                                DateParseHandling = DateParseHandling.DateTimeOffset
                           };
            return JsonConvert.DeserializeObject<Response<TResponse>>(content, settings).Data;
        }
    }
}