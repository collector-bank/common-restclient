// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAuthorizationHeaderFactory.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Authorization
{
    using System;

    using Collector.Common.RestContracts;

    public interface IAuthorizationHeaderFactory
    {
        /// <summary>
        /// Builds the header value to be used in the Authorization http header. 
        /// </summary>
        /// <param name="body">Request Body</param>
        /// <param name="uri">Requested URI</param>
        /// <param name="httpMethod">HTTP request method</param>
        /// <returns></returns>
        string Get(string body, Uri uri, HttpMethod httpMethod);
    }
}