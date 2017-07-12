// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRestSharpClientWrapper.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Interfaces
{
    using System;

    using Collector.Common.RestContracts.Interfaces;

    using RestSharp;

    internal interface IRestSharpClientWrapper
    {
        void ExecuteAsync(IRestRequest restRequest, IRequest request, Action<IRestResponse> callback);
    }
}