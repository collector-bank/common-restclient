// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IApiClientBuilder.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient
{
    using Collector.Common.RestClient.Interfaces;

    using Serilog;

    public interface IApiClientBuilder
    {
        /// <summary>
        /// Configure the IRestApiClient by RestContract key
        /// </summary>
        /// <param name="contractKey">The key which identifies requests for contracts</param>
        /// <param name="baseUrl">Api base url</param>
        /// <param name="authorizationHeaderFactory">(optional) Authorization header factory creating the Authorization header for the request</param>
        /// <returns></returns>
        IApiClientBuilder ConfigureContractByKey(string contractKey, string baseUrl, IAuthorizationHeaderFactory authorizationHeaderFactory = null);

        /// <summary>
        /// Using RestSharp client as the request provider
        /// </summary>
        /// <returns></returns>
        IApiClientBuilder UseRestSharp();
        
        /// <summary>
        /// Configures serilog for all requests made by the IRestApiClient that's beeing built
        /// </summary>
        /// <param name="logger">Configured ILogger</param>
        /// <returns></returns>
        IApiClientBuilder ConfigureLogging(ILogger logger);

        /// <summary>
        /// Builds a configured IRestApiClient, based on currently configured configurations
        /// </summary>
        /// <returns>Fully configured IRestApiClient</returns>
        IRestApiClient Build();
    }
}