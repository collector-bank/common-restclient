// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestSharpApiClientBuilder.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient
{
    using Collector.Common.RestClient.Implementation;

    using Interfaces;

    using RestSharp.Authenticators;

    public class RestSharpApiClientBuilder : BaseBuilder
    {
        private IAuthenticator _authenticator;

        public RestSharpApiClientBuilder UseAuthorizationHeaderFactory(IAuthorizationHeaderFactory authorizationHeaderFactory)
        {
            _authenticator = new RestSharpAuthenticator(authorizationHeaderFactory);
            return this;
        }

        public override IRestApiClient Build()
        {
            var wrapper = new RestSharpClientWrapper(_authenticator, BaseUrl);

            var restSharpClient = new RestSharpApiRequestApiClient(wrapper);

            return new RestApiClient(restSharpClient, Logger);
        }
    }
}