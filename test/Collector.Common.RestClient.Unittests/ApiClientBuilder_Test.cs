﻿namespace Collector.Common.RestClient.UnitTests
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
                .AddJsonFile("appsettings.json")
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
        public void It_can_replace_or_add_authorization_configuration()
        {
            var clientId = Guid.NewGuid().ToString();
            var authorizationConfiguration = new Oauth2AuthorizationConfiguration(clientId, "secret", "aud", "issuer", "scopes");
            var provider = new ApiClientBuilder()
                           .ConfigureFromAppSettings()
                           .WithAuthorizationConfiguration("MyApi", authorizationConfiguration)
                           .WithAuthorizationConfiguration("MyOtherApi", authorizationConfiguration)
                           .Build();

            Assert.NotNull(provider);
        }
    }
}