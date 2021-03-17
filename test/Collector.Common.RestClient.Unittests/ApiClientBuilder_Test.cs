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
        public void It_can_be_configured_through_either_configuration_sections_or_app_settings()
        {
            var section = new ConfigurationBuilder()
                .AddJsonFile("configuration.json")
                .Build()
                .GetSection("RestClient");

            var provider = new ApiClientBuilder()
                .ConfigureFromConfigSection(section)
                .Build();
        }


        [Test]
        public void It_can_register_authenticator_builders()
        {
            var clientId = Guid.NewGuid().ToString();
            
            var section = new ConfigurationBuilder()
                          .AddJsonFile("configurationWithCustomAuthenticator.json")
                          .Build()
                          .GetSection("RestClient");

            var provider = new ApiClientBuilder()
                           .RegisterAuthenticator("MyCustomAuth", configReader => new Oauth2AuthorizationConfiguration(clientId, "secret", "aud", "issuer", "scopes"))
                           .ConfigureFromConfigSection(section)
                           .Build();

            Assert.NotNull(provider);
        }
    }
}