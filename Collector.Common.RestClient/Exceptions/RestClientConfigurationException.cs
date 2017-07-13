// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestClientConfigurationException.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Exceptions
{
    using System;

    public class RestClientConfigurationException : Exception
    {
        public RestClientConfigurationException(string message)
            : base(message)
        {
        }
    }
}