// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseBuilder.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient
{
    using Collector.Common.RestClient.Interfaces;

    using Serilog;

    public abstract class BaseBuilder
    {
        protected string BaseUrl { get; set; }

        protected ILogger Logger { get; set; }

        public BaseBuilder UseLogging(ILogger logger)
        {
            Logger = logger;
            return this;
        }

        public BaseBuilder UseBaseUrl(string baseUrl)
        {
            BaseUrl = baseUrl;
            return this;
        }

        public abstract IRestApiClient Build();
    }
}