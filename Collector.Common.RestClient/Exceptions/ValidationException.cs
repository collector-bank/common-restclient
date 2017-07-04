namespace Collector.Common.RestClient.Exceptions
{
    using System;
    using System.Collections.Generic;

    using Collector.Common.RestContracts;

    /// <summary>
    /// A contract violation was found before tha api was called.
    /// </summary>
    public class ValidationException : Exception
    {
        public IEnumerable<ErrorInfo> ErrorInfos { get; }
        
        internal ValidationException(IEnumerable<ErrorInfo> errorInfos)
            : base("Validation errors occered, check ErrorInfos property for more information")
        {
            ErrorInfos = errorInfos;
        }
    }
}