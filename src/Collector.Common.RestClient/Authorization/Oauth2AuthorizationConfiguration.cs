namespace Collector.Common.RestClient.Authorization
{
    using System;

    using Collector.Common.RestClient.Configuration;

    using Serilog;
    
    public class Oauth2AuthorizationConfiguration : IAuthorizationConfiguration
    {
        internal string Issuer { get; }
        internal string ClientId { get; }
        internal string ClientSecret { get; }
        internal string Audience { get; }
        internal string Scopes { get; set; }

        public Oauth2AuthorizationConfiguration(string clientId, string clientSecret, string audience, string issuer, string scopes)
        {
            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentNullException(nameof(clientId));
            if (string.IsNullOrEmpty(clientSecret))
                throw new ArgumentNullException(nameof(clientSecret));
            if (string.IsNullOrEmpty(audience))
                throw new ArgumentNullException(nameof(audience));
            if (string.IsNullOrEmpty(issuer))
                throw new ArgumentNullException(nameof(issuer));

            ClientId = clientId;
            ClientSecret = clientSecret;
            Audience = audience;
            Issuer = issuer;
            Scopes = scopes;
        }

        internal Oauth2AuthorizationConfiguration(IConfigReader configReader)
        {
            Audience = configReader.GetAndEnsureString("Audience", onlyFromSubSection: true);
            Issuer = configReader.GetAndEnsureString("Issuer");
            ClientId = configReader.GetAndEnsureString("ClientId");
            ClientSecret = configReader.GetAndEnsureString("ClientSecret");
            Scopes = configReader.GetString("Scopes");
        }

        public IAuthorizationHeaderFactory CreateFactory(ILogger logger)
        {
            return new Oauth2AuthorizationHeaderFactory(this, logger);
        }
    }
}