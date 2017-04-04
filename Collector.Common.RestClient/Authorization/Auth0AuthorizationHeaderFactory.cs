// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Auth0AuthorizationHeaderFactory.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Authorization
{
    using System;
    using System.Net;

    using Collector.Common.Jwt;
    using Collector.Common.RestClient.Exceptions;
    using Collector.Common.RestContracts;

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
        private readonly string _apiIdentifier;
        private string _token;

        public Auth0AuthorizationHeaderFactory(string clientId, string clientSecret, string apiIdentifier)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;

            _apiIdentifier = apiIdentifier;

            if (_apiIdentifier.EndsWith("/"))
            {
                _apiIdentifier = _apiIdentifier.Remove(_apiIdentifier.Length - 1);
            }
        }

        public string Get(string body, Uri uri, HttpMethod httpMethod)
        {
            var token = GetJwtToken();
            return $"Bearer {token}";
        }

        private string GetJwtToken()
        {
            if (HasExpired())
            {
                _token = GetNewJwtToken();
            }

            return _token;
        }

        private string GetNewJwtToken()
        {
            var client = new RestClient(URL_COLLECTOR_AUTH0);
            var request = new RestRequest(Method.POST);
            request.AddHeader(HEADER_CONTENT_TYPE, APP_JSON);

            var paramJsonValue = string.Format(REQUEST_BODY, _clientId, _clientSecret, _apiIdentifier);
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
                dynamic data = JsonConvert.DeserializeObject(auth0Response.Content);
                return data.access_token.ToString();
            }
            catch
            {
                throw new AuthException("Cannot parse the auth0 response!", AUTH0_ERROR);
            }
        }

        private bool HasExpired()
        {
            return string.IsNullOrWhiteSpace(_token) || JwtExpirationValidator.HasExpired(_token);
        }
    }
}