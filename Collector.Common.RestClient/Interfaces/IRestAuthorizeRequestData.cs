// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRestAuthorizeRequestData.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Interfaces
{
    using System;

    using Collector.Common.RestContracts;

    public interface IRestAuthorizeRequestData
    {
        string Body { get; }

        Uri Uri { get; }

        HttpMethod HttpMethod { get; }
    }
}