// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRestSharpClientWrapper.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Interfaces
{
    using System;

    using RestSharp;
    using RestSharp.Authenticators;

    internal interface IRestSharpClientWrapper
    {
        IAuthenticator Authenticator { get; set; }

        void ExecuteAsync(IRestRequest request, Action<IRestResponse> callback);
    }
}