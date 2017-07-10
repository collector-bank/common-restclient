// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthException.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Exceptions
{
    using System;

    public class AuthException : Exception
    {
        public AuthException(string message) : base(message)
        {
        }
    }
}
