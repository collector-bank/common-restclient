namespace Collector.Common.RestClient.Logging
{
    using System;

    using RestSharp;

    public class ResponseLogEntry
    {
        public Uri HttpRequestUrl { get; set; }

        public Method HttpRequestType { get; set; }

        public int StatusCode { get; set; }

        public string MediaType { get; set; }

        public int ResponseTimeMilliseconds { get; set; }

        public int ResponseContentLength { get; set; }

        public string RawResponseBody { get; set; }

        public string ResponseBody { get; set; }
    }
}