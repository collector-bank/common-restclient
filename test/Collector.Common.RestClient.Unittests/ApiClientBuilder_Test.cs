namespace Collector.Common.RestClient.UnitTests
{
    using System;

    using Collector.Common.RestClient.Authorization;

    using Microsoft.Extensions.Configuration;

    using NUnit.Framework;

    [TestFixture]
    public class ApiClientBuilder_Test
    {
        [Test]
        public void It_can_be_configured_through_config_section()
        {
            var section = new ConfigurationBuilder()
                .AddJsonFile("configuration.json")
                .Build()
                .GetSection("RestClient");

            var provider = new ApiClientBuilder()
                .ConfigureFromConfigSection(section)
                .Build();

            Assert.NotNull(provider);
        }

        [Test]
        public void It_can_be_configured_through_app_settings()
        {
            var provider = new ApiClientBuilder()
                .ConfigureFromAppSettings()
                .Build();

            Assert.NotNull(provider);
        }

        [Test]
        public void It_can_register_authenticator_builders()
        {
            var clientId = Guid.NewGuid().ToString();

            var provider = new ApiClientBuilder()
                           .ConfigureFromAppSettings()
                           .RegisterAuthenticator("MyCustomAuth", configReader => new Oauth2AuthorizationConfiguration(clientId, "secret", "aud", "issuer", "scopes"))
                           .Build();

            Assert.NotNull(provider);
        }
    }
}