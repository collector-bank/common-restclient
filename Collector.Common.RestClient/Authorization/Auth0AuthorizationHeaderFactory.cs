// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Auth0AuthorizationHeaderFactory.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Authorization
{
    using System;
    using System.Net;

    using Collector.Common.RestClient.Exceptions;
    using Collector.Common.RestClient.Interfaces;

    using Newtonsoft.Json;

    using RestSharp;

    public class Auth0AuthorizationHeaderFactory : IAuthorizationHeaderFactory
    {
        public const string AUTH0_ERROR = "AUTH0_ERROR";
        private const string REQUEST_BODY = "{{\"client_id\":\"{0}\"," +
                                     "\"client_secret\":\"{1}\"," +
                                     "\"audience\":\"{2}\"," +
                                     "\"grant_type\":\"client_credentials\"}}";

        private const string APP_JSON = "application/json";
        private const string HEADER_CONTENT_TYPE = "Content-Type";
        private const string URL_COLLECTOR_AUTH0 = "https://collectorbank.eu.auth0.com/oauth/token";
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _audience;
        private string _token;
        private DateTimeOffset _expiration;
        private string _tokenType;

        public Auth0AuthorizationHeaderFactory(string clientId, string clientSecret, string audience)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;

            _audience = audience;

            if (_audience.EndsWith("/"))
            {
                _audience = _audience.Remove(_audience.Length - 1);
            }
        }

        public string Get(IRestAuthorizeRequestData restAuthorizeRequestData)
        {
            var token = GetToken();
            return $"{_tokenType} {token}";
        }

        private string GetToken()
        {
            if (DateTimeOffset.UtcNow > _expiration)
                GetNewToken();

            return _token;
        }

        private void GetNewToken()
        {
            var client = new RestClient(URL_COLLECTOR_AUTH0);
            var request = new RestRequest(Method.POST);
            request.AddHeader(HEADER_CONTENT_TYPE, APP_JSON);

            var paramJsonValue = string.Format(REQUEST_BODY, _clientId, _clientSecret, _audience);
            request.AddParameter(APP_JSON, paramJsonValue, ParameterType.RequestBody);
            var auth0Response = client.Execute(request);

            if (auth0Response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedException();
            }

            if (auth0Response.StatusCode != HttpStatusCode.OK)
            {
                throw new AuthException(auth0Response.Content, AUTH0_ERROR);
            }

            try
            {
                var data = JsonConvert.DeserializeObject<OauthTokenResponse>(auth0Response.Content);
                _token = data.access_token;
                _expiration = DateTimeOffset.UtcNow.AddSeconds(data.expires_in - 10);
                _tokenType = data.token_type;
            }
            catch
            {
                throw new AuthException("Cannot parse the auth0 response!", AUTH0_ERROR);
            }
        }

        private class OauthTokenResponse
        {
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string token_type { get; set; }
        }
    }
}