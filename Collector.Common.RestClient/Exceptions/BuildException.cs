// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuildException.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Exceptions
{
    using System;

    using Collector.Common.Library.Retry;

    public class BuildException : Exception, IRetrySuppressingException
    {
        public BuildException(string message)
            : base(message)
        {
        }
    }
}