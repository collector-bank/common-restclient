// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRestSharpClientWrapper.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Interfaces
{
    using System;

    using RestSharp;

    internal interface IRestSharpClientWrapper
    {
        void ExecuteAsync(IRestRequest request, string contractIdentifier, Action<IRestResponse> callback);
    }
}