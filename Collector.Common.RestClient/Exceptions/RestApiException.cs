// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestApiException.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Exceptions
{
    using System;

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
        /// <param name="message">The message.</param>
        /// <param name="errorCode">The error code.</param>
        public RestApiException(string message, string errorCode)
            : base(message: message)
        {
            _errorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestApiException"/> class.
        /// </summary>
        /// <param name="restError">The rest error.</param>
        public RestApiException(Error restError)
            : base(message: restError.Message, innerException: null)
        {
            _errorCode = restError.Code;
        }

        /// <summary>
        /// The error code.
        /// </summary>
        public string ErrorCode => _errorCode ?? Message;
    }
}