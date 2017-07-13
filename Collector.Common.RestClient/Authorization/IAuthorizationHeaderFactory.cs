// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAuthorizationHeaderFactory.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Authorization
{
    public interface IAuthorizationHeaderFactory
    {
        /// <summary>
        /// Builds the header value to be used in the Authorization http header. 
        /// </summary>
        /// <param name="restAuthorizeRequestData">The request data to build Authorization for.</param>
        /// <returns></returns>
        string Get(RestAuthorizeRequestData restAuthorizeRequestData);
    }
}