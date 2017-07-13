// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAuthorizationConfiguration.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Collector.Common.RestClient.Authorization
{
    using Serilog;

    public interface IAuthorizationConfiguration
    {
        IAuthorizationHeaderFactory CreateFactory(ILogger logger);
    }
}