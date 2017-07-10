// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Oauth2AuthorizationHeaderFactory.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Authorization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    using Collector.Common.RestClient.Exceptions;
    using Collector.Common.RestClient.Interfaces;

    using Newtonsoft.Json;

    using RestSharp;

    public class Oauth2AuthorizationHeaderFactory : IAuthorizationHeaderFactory
    {
        private readonly string _issuer;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _audience;
        private string _token;
        private DateTimeOffset _expiration;
        private string _tokenType;

        public Oauth2AuthorizationHeaderFactory(string clientId, string clientSecret, string audience, string issuer)
        {
            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentNullException(nameof(clientId));
            if (string.IsNullOrEmpty(clientSecret))
                throw new ArgumentNullException(nameof(clientSecret));
            if (string.IsNullOrEmpty(audience))
                throw new ArgumentNullException(nameof(audience));
            if (string.IsNullOrEmpty(issuer))
                throw new ArgumentNullException(nameof(issuer));

            _clientId = clientId;
            _clientSecret = clientSecret;
            _audience = audience;
            _issuer = issuer;
        }

        internal Oauth2AuthorizationHeaderFactory(string contractKey)
        {
            _audience = ConfigReader.GetAndEnsureValueFromAppSettingsKey<string>($"RestClient:Audience.{contractKey}");

            _issuer = ConfigReader.GetValueFromAppSettingsKey<string>($"RestClient:Issuer.{contractKey}") ?? ConfigReader.GetValueFromAppSettingsKey<string>("RestClient:Issuer");
            _clientId = ConfigReader.GetValueFromAppSettingsKey<string>($"RestClient:ClientId.{contractKey}") ?? ConfigReader.GetValueFromAppSettingsKey<string>("RestClient:ClientId");
            _clientSecret = ConfigReader.GetValueFromAppSettingsKey<string>($"RestClient:ClientSecret.{contractKey}") ?? ConfigReader.GetValueFromAppSettingsKey<string>("RestClient:ClientSecret");

            if (_issuer == null)
                throw new KeyNotFoundException($"Could not find issuer in config, either configure a value for key 'RestClient:Issuer' or key 'RestClient:Issuer.{contractKey}'");
            
            if (_clientId == null)
                throw new KeyNotFoundException($"Could not find client id in config, either configure a value for key 'RestClient:ClientId' or key 'RestClient:ClientId.{contractKey}'");

            if (_clientSecret == null)
                throw new KeyNotFoundException($"Could not find client secret in config, either configure a value for key 'RestClient:ClientSecret' or key 'RestClient:ClientSecret.{contractKey}'");
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
            var client = new RestClient(_issuer);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddHeader("Accept", "application/json");

            var requestBodyParameters = new Dictionary<string, string>
                             {
                                 ["client_id"] = _clientId,
                                 ["client_secret"] = _clientSecret,
                                 ["audience"] = _audience,
                                 ["grant_type"] = "client_credentials"
                             };

            var requestBody = string.Join("&", requestBodyParameters.Select(kvp => $"{kvp.Key}={kvp.Value}"));

            request.AddParameter("application/x-www-form-urlencoded", requestBody, ParameterType.RequestBody);
            
            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedException();
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new AuthException(response.Content);
            }

            try
            {
                var data = JsonConvert.DeserializeObject<OauthTokenResponse>(response.Content);
                _token = data.access_token;
                _expiration = DateTimeOffset.UtcNow.AddSeconds(data.expires_in - 10);
                _tokenType = data.token_type;
            }
            catch
            {
                throw new AuthException("Cannot parse the oauth response.");
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