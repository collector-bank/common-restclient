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
        private readonly string _errorCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestApiException"/> class.
        /// </summary>
        /// <param name="httpStatusCode"></param>
        /// <param name="message">The message.</param>
        /// <param name="errorCode">The error code.</param>
        internal RestApiException(HttpStatusCode httpStatusCode, string message, string errorCode)
            : base(message: message)
        {
            _errorCode = errorCode;
            HttpStatusCode = httpStatusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestApiException"/> class.
        /// </summary>
        /// <param name="httpStatusCode"></param>
        /// <param name="restError">The rest error.</param>
        internal RestApiException(HttpStatusCode httpStatusCode, Error restError)
            : base(message: restError.Message, innerException: null)
        {
            _errorCode = restError.Code;
            HttpStatusCode = httpStatusCode;
            Errors = restError.Errors;
        }

        /// <summary>
        /// The error code.
        /// </summary>
        public string ErrorCode => _errorCode ?? Message;

        public string Reason => Errors?.First()?.Reason;

        public HttpStatusCode HttpStatusCode { get; }

        public IEnumerable<ErrorInfo> Errors { get; }
    }
}