namespace Collector.Common.RestClient.Logging
{
    using RestSharp;

    public class RequestLogEntry
    {
        public string RequestContent { get; set; }

        public string HttpRequestUrl { get; set; }

        public Method HttpRequestType { get; set; }

    }
}