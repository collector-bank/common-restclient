// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestApiException.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    using Collector.Common.RestContracts;

    /// <summary>
    /// Generic exception with error code.
    /// </summary>
    public class RestApiException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestApiException"/> class.
        /// </summary>
        /// <param name="httpStatusCode"></param>
        /// <param name="message">The message.</param>
        internal RestApiException(HttpStatusCode httpStatusCode, string message)
            : base(message: message)
        {
            HttpStatusCode = httpStatusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestApiException"/> class.
        /// </summary>
        /// <param name="httpStatusCode"></param>
        /// <param name="restError">The rest error.</param>
        internal RestApiException(HttpStatusCode httpStatusCode, Error restError)
            : base(message: restError.Message)
        {
            HttpStatusCode = httpStatusCode;
            Error = restError;
        }

        public HttpStatusCode HttpStatusCode { get; }

        public Error Error { get; }
    }
}