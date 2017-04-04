// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnauthorizedException.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// <summary>
//   Defines the UnauthorizedException type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Exceptions
{
    public class UnauthorizedException : AuthException
    {
        private const string ERROR_MESSAGE = "Access denied. Please check again the client id and client secret.";

        public UnauthorizedException()
            : base(ERROR_MESSAGE, string.Empty)
        {
        }
    }
}
