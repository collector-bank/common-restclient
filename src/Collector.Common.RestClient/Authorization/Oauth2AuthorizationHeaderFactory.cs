namespace Collector.Common.RestClient.Authorization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    using Collector.Common.RestClient.Exceptions;

    using Newtonsoft.Json;

    using RestSharp;

    using Serilog;

    internal class Oauth2AuthorizationHeaderFactory : IAuthorizationHeaderFactory
    {
        private readonly Oauth2AuthorizationConfiguration _configuration;
        private readonly ILogger _logger;
        private string _token;
        private DateTimeOffset _expiration;
        private DateTimeOffset _expirationWithGracePeriod;
        private string _tokenType;

        public Oauth2AuthorizationHeaderFactory(Oauth2AuthorizationConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger?.ForContext(GetType());
        }

        public string Get(RestAuthorizeRequestData restAuthorizeRequestData)
        {
            var token = GetToken();
            return $"{_tokenType} {token}";
        }

        private string GetToken()
        {
            if (DateTimeOffset.UtcNow > _expiration)
                GetNewToken();
            else if (DateTimeOffset.UtcNow > _expirationWithGracePeriod)
            {
                try
                {
                    GetNewToken();
                }
                catch (Exception e)
                {
                    _logger?.Warning(e, "Will continue to use old token which is valid for {TokenValidityTimeInSeconds} more seconds",  (int)(_expiration - DateTimeOffset.UtcNow).TotalSeconds);
                }
            }

            return _token;
        }

        private void GetNewToken()
        {
            var client = new RestClient(_configuration.Issuer);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddHeader("Accept", "application/jwt, application/json");

            var requestBodyParameters = new Dictionary<string, string>
                             {
                                 ["client_id"] = _configuration.ClientId,
                                 ["client_secret"] = _configuration.ClientSecret,
                                 ["audience"] = _configuration.Audience,
                                 ["grant_type"] = "client_credentials"
                             };

            if (!string.IsNullOrWhiteSpace(_configuration.Scopes))
            {
                requestBodyParameters.Add("scope", _configuration.Scopes);
            }

            var requestBody = string.Join("&", requestBodyParameters.Select(kvp => $"{kvp.Key}={WebUtility.UrlEncode(kvp.Value)}"));

            request.AddParameter("application/x-www-form-urlencoded", requestBody, ParameterType.RequestBody);
            
            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger?.ForContext("IssuerResponse", response.Content)
                       ?.Warning("There was a problem getting oauth2 token from the issuer {Issuer}", _configuration.Issuer);
                throw new RestClientCallException(HttpStatusCode.Unauthorized, "Access denied, please verify oauth2 configuration.");
            }

            try
            {
                var data = JsonConvert.DeserializeObject<OauthTokenResponse>(response.Content);
                _token = data.access_token;
                _expiration = DateTimeOffset.UtcNow.AddSeconds(data.expires_in);
                _expirationWithGracePeriod = DateTimeOffset.UtcNow.AddSeconds(data.expires_in * 0.8);
                _tokenType = data.token_type;
                _logger?.Information("Successfully fetched oath2 token from the issuer {Issuer}", _configuration.Issuer);
            }
            catch
            {
                throw new RestClientCallException(HttpStatusCode.Unauthorized, "Cannot parse the oauth2 response.");
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