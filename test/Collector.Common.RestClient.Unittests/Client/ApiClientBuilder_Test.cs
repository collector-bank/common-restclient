namespace Collector.Common.RestClient.UnitTests.Client
{
    using System;
    using System.Collections.Generic;

    using AutoFixture;

    using Collector.Common.RestClient.Authorization;
    using Collector.Common.RestClient.Exceptions;

    using Moq;

    using NUnit.Framework;

    using RestSharp;

    [TestFixture]
    public class ApiClientBuilder_Test
    {
        private ApiClientBuilder _sut;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _sut = new ApiClientBuilder();
            _fixture = new Fixture();
        }

        [Test]
        public void When_api_builder_is_configured_with_contract_it_will_contain_the_contract()
        {
            var contract = _fixture.Create<string>();
            var endpoint = _fixture.Create<Uri>();

            var builder = _sut.ConfigureContractByKey(contract, endpoint.ToString());

            CollectionAssert.Contains(builder.BaseUris.Keys, contract);
        }

        [Test]
        public void When_api_builder_is_configured_with_contract_it_will_contain_the_base_uri()
        {
            var contract = _fixture.Create<string>();
            var endpoint = _fixture.Create<Uri>();

            var builder = _sut.ConfigureContractByKey(contract, endpoint.ToString());

            Assert.AreEqual(endpoint, builder.BaseUris[contract]);
        }


        [Test]
        public void When_configure_the_contract_multiple_times_it_will_throw_exception()
        {
            var contract = _fixture.Create<string>();
            var endpoint = _fixture.Create<Uri>();


            Assert.Throws<RestClientConfigurationException>(() =>
                                          {
                                              _sut.ConfigureContractByKey(contract, endpoint.ToString())
                                                  .ConfigureContractByKey(contract, endpoint.ToString());
                                          });
        }

        [Test]
        public void When_configure_contract_with_null_it_will_throw_exception()
        {
            var endpoint = _fixture.Create<string>();

            Assert.Throws<ArgumentNullException>(() =>
                                          {
                                              _sut.ConfigureContractByKey(null, endpoint);
                                          });
        }

        [Test]
        public void When_api_builder_is_configured_with_authenticator_it_will_hold_the_authenticator()
        {
            var contract = _fixture.Create<string>();
            var endpoint = _fixture.Create<Uri>();
            var authorizationConfiguration = new Mock<IAuthorizationConfiguration>();

            var reqStub = _fixture.Freeze<Mock<IRestRequest>>();
            reqStub.Setup(x => x.Parameters).Returns(new List<Parameter>());

            var builder = _sut.ConfigureContractByKey(contract, endpoint.ToString(), authorizationConfiguration.Object);

            var configuredAuthorizationHeaderFactory = builder.Authenticators[contract];

            Assert.AreSame(authorizationConfiguration.Object, configuredAuthorizationHeaderFactory); // WTF
        }

        [Test]
        public void When_building_it_will_throw_exception_if_no_contracts_are_configured()
        {
            Assert.Throws<RestClientConfigurationException>(() => _sut.Build());
        }
    }
}
