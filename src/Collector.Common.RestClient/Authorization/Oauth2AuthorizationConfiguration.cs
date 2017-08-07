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

        public Oauth2AuthorizationConfiguration(string clientId, string clientSecret, string audience, string issuer)
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
        }

        internal Oauth2AuthorizationConfiguration(IConfigReader configReader)
        {
            Audience = configReader.GetAndEnsureString("Audience", onlyFromSubSection: true);

            Issuer = configReader.GetAndEnsureString("Issuer");
            ClientId = configReader.GetAndEnsureString("ClientId");
            ClientSecret = configReader.GetAndEnsureString("ClientSecret");
        }

        public IAuthorizationHeaderFactory CreateFactory(ILogger logger)
        {
            return new Oauth2AuthorizationHeaderFactory(this, logger);
        }
    }
}