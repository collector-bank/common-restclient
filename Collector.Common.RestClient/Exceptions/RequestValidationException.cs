// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequestValidationException.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Exceptions
{
    using System;
    using System.Collections.Generic;

    using Collector.Common.RestContracts;

    /// <summary>
    /// A contract violation was found before the api was called.
    /// </summary>
    public class RequestValidationException : Exception
    {
        public IEnumerable<ErrorInfo> ErrorInfos { get; }
        
        internal RequestValidationException(IEnumerable<ErrorInfo> errorInfos)
            : base("Validation errors occurred, check ErrorInfos property for more information")
        {
            ErrorInfos = errorInfos;
        }
    }
}