// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuildException.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Exceptions
{
    using System;

    public class BuildException : Exception
    {
        public BuildException(string message)
            : base(message)
        {
        }
    }
}