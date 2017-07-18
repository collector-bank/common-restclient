namespace Collector.Common.RestClient.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    using Collector.Common.RestContracts;

    /// <summary>
    /// Generic exception with error code.
    /// </summary>
    public class RestClientCallException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestClientCallException"/> class.
        /// </summary>
        /// <param name="httpStatusCode"></param>
        /// <param name="message">The message.</param>
        public RestClientCallException(HttpStatusCode httpStatusCode, string message, Exception innerException = null)
            : base(message: message, innerException: innerException)
        {
            HttpStatusCode = httpStatusCode;
            Error = new Error($"{(int)httpStatusCode}", httpStatusCode.ToString());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestClientCallException"/> class.
        /// </summary>
        /// <param name="httpStatusCode"></param>
        /// <param name="restError">The rest error.</param>
        public RestClientCallException(HttpStatusCode httpStatusCode, Error restError)
            : base(message: restError.Message)
        {
            HttpStatusCode = httpStatusCode;
            Error = restError;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestClientCallException"/> class.
        /// </summary>
        /// <param name="httpStatusCode"></param>
        /// <param name="errorInfos">The rest error.</param>
        public RestClientCallException(HttpStatusCode httpStatusCode, IEnumerable<ErrorInfo> errorInfos)
            : base(message: httpStatusCode.ToString())
        {
            HttpStatusCode = httpStatusCode;
            Error = new Error($"{(int)httpStatusCode}", httpStatusCode.ToString(), errorInfos);
        }

        public HttpStatusCode HttpStatusCode { get; }

        public Error Error { get; }
    }
}