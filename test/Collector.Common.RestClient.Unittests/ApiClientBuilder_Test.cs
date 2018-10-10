namespace Collector.Common.RestClient.UnitTests
{
    using System;

    using Collector.Common.RestClient.Authorization;

#if NETCOREAPP2_0
    using Microsoft.Extensions.Configuration;
#endif

    using NUnit.Framework;

    [TestFixture]
    public class ApiClientBuilder_Test
    {
        [Test]
        public void It_can_be_configured_through_either_configuration_sections_or_app_settings()
        {
#if NETCOREAPP2_0
            var section = new ConfigurationBuilder()
                .AddJsonFile("configuration.json")
                .Build()
                .GetSection("RestClient");

            var provider = new ApiClientBuilder()
                .ConfigureFromConfigSection(section)
                .Build();
#elif NET452
            var provider = new ApiClientBuilder()
                .ConfigureFromAppSettings()
                .Build();
#endif
            Assert.NotNull(provider);
        }


        [Test]
        public void It_can_register_authenticator_builders()
        {
            var clientId = Guid.NewGuid().ToString();

#if NETCOREAPP2_0
            var section = new ConfigurationBuilder()
                          .AddJsonFile("configuration.json")
                          .Build()
                          .GetSection("RestClient");

            var provider = new ApiClientBuilder()
                           .ConfigureFromConfigSection(section)
                           .RegisterAuthenticator("MyCustomAuth", configReader => new Oauth2AuthorizationConfiguration(clientId, "secret", "aud", "issuer", "scopes"))
                           .Build();
#elif NET452
            var provider = new ApiClientBuilder()
                           .ConfigureFromAppSettings()
                           .RegisterAuthenticator("MyCustomAuth", configReader => new Oauth2AuthorizationConfiguration(clientId, "secret", "aud", "issuer", "scopes"))
                           .Build();
#endif

            Assert.NotNull(provider);
        }
    }
}