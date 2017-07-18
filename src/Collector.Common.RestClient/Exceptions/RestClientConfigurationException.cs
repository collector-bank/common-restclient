namespace Collector.Common.RestClient.Exceptions
{
    using System;

    public class RestClientConfigurationException : Exception
    {
        public RestClientConfigurationException(string message)
            : base(message)
        {
        }
    }
}