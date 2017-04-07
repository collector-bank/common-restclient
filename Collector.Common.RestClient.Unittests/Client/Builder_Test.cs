// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Builder_Test.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.UnitTests.Client
{
    using System;
    using System.Collections.Generic;

    using Collector.Common.RestClient.Exceptions;
    using Collector.Common.RestClient.Interfaces;
    using Collector.Common.UnitTest.Helpers;
    using Collector.Common.UnitTest.Helpers.Autofixture;

    using NUnit.Framework;

    using Ploeh.AutoFixture;

    using RestSharp;

    using Rhino.Mocks;

    [TestFixture]
    public class Builder_Test : BaseUnitTest<CommonFixture>
    {
        [Test]
        public void When_api_builder_is_configured_with_contract_it_will_contain_the_contract()
        {
            var contract = Fixture.Create<string>();
            var endpoint = Fixture.Create<string>();

            var builder = (ApiClientBuilder)new ApiClientBuilder()
                .ConfigureContractByKey(contract, endpoint);

            CollectionAssert.Contains(builder.BaseUris.Keys, contract);
        }

        [Test]
        public void When_api_builder_is_configured_with_contract_it_will_contain_the_base_uri()
        {
            var contract = Fixture.Create<string>();
            var endpoint = Fixture.Create<string>();

            var builder = (ApiClientBuilder)new ApiClientBuilder()
                .ConfigureContractByKey(contract, endpoint);

            Assert.AreEqual(endpoint, builder.BaseUris[contract]);
        }


        [Test]
        public void When_configure_the_contract_multiple_times_it_will_throw_exception()
        {
            var contract = Fixture.Create<string>();
            var endpoint = Fixture.Create<string>();


            Assert.Throws<BuildException>(() =>
                                          {
                                              new ApiClientBuilder()
                                                  .ConfigureContractByKey(contract, endpoint)
                                                  .ConfigureContractByKey(contract, endpoint);
                                          });

        }

        [Test]
        public void When_configure_contract_with_null_it_will_throw_exception()
        {
            var endpoint = Fixture.Create<string>();

            Assert.Throws<ArgumentNullException>(() =>
                                          {
                                              new ApiClientBuilder()
                                                  .ConfigureContractByKey(null, endpoint);
                                          });

        }

        [Test]
        public void When_api_builder_is_configured_with_authenticator_it_will_hold_the_authenticator()
        {
            var contract = Fixture.Create<string>();
            var endpoint = Fixture.Create<string>();
            var authorizationHeaderFactory = Fixture.Create<IAuthorizationHeaderFactory>();
            Fixture.Create<IRestRequest>().Stub(x => x.Parameters).Return(new List<Parameter>());

            var builder = (ApiClientBuilder)new ApiClientBuilder()
                .ConfigureContractByKey(contract, endpoint, authorizationHeaderFactory);

            var configuredAuthorizationHeaderFactory = builder.Authenticators[contract];

            Assert.AreSame(configuredAuthorizationHeaderFactory, configuredAuthorizationHeaderFactory);
        }

        [Test]
        public void When_building_it_will_throw_exception_if_no_contracts_are_configured()
        {
            Assert.Throws<BuildException>(() =>
                                          {
                                              new ApiClientBuilder().ConfigureContractByKey(Fixture.Create<string>(), Fixture.Create<string>()).Build();
                                          });
        }

        [Test]
        public void When_building_it_will_throw_exception_if_no_rest_client_is_configured()
        {
            Assert.Throws<BuildException>(() =>
                                          {
                                              new ApiClientBuilder().ConfigureContractByKey(Fixture.Create<string>(), Fixture.Create<string>()).Build();
                                          });
        }
    }
}
