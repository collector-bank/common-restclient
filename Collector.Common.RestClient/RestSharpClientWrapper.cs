// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestSharpClientWrapper.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient
{
    using System;

    using Collector.Common.RestClient.Interfaces;

    using RestSharp;
    using RestSharp.Authenticators;

    /// <summary>
    /// The rest sharp client wrapper.
    /// </summary>
    internal sealed class RestSharpClientWrapper : IRestSharpClientWrapper
    {
        public IAuthenticator Authenticator { get; set; }

        public void ExecuteAsync(IRestRequest request, Action<IRestResponse> callback)
        {
            var client = new RestClient { Authenticator = Authenticator };

            client.ExecuteAsync(request, callback);
        }
    }
}