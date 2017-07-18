namespace Collector.Common.RestClient.UnitTests.Client
{
    using System;
    using System.Collections.Generic;

    using Collector.Common.RestClient.Authorization;
    using Collector.Common.RestClient.Exceptions;

    using NUnit.Framework;

    using Ploeh.AutoFixture;

    using RestSharp;

    using Rhino.Mocks;

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
            var endpoint = _fixture.Create<string>();

            var builder = _sut.ConfigureContractByKey(contract, endpoint);

            CollectionAssert.Contains(builder.BaseUris.Keys, contract);
        }

        [Test]
        public void When_api_builder_is_configured_with_contract_it_will_contain_the_base_uri()
        {
            var contract = _fixture.Create<string>();
            var endpoint = _fixture.Create<string>();

            var builder = _sut.ConfigureContractByKey(contract, endpoint);

            Assert.AreEqual(endpoint, builder.BaseUris[contract]);
        }


        [Test]
        public void When_configure_the_contract_multiple_times_it_will_throw_exception()
        {
            var contract = _fixture.Create<string>();
            var endpoint = _fixture.Create<string>();


            Assert.Throws<RestClientConfigurationException>(() =>
                                          {
                                              _sut.ConfigureContractByKey(contract, endpoint)
                                                  .ConfigureContractByKey(contract, endpoint);
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
            var endpoint = _fixture.Create<string>();
            var authorizationConfiguration = MockRepository.GenerateMock<IAuthorizationConfiguration> ();

            var reqStub = MockRepository.GenerateMock<IRestRequest>();
            reqStub.Stub(x => x.Parameters).Return(new List<Parameter>());

            var builder = _sut.ConfigureContractByKey(contract, endpoint, authorizationConfiguration);

            var configuredAuthorizationHeaderFactory = builder.Authenticators[contract];

            Assert.AreSame(authorizationConfiguration, configuredAuthorizationHeaderFactory); // WTF
        }

        [Test]
        public void When_building_it_will_throw_exception_if_no_contracts_are_configured()
        {
            Assert.Throws<RestClientConfigurationException>(() => _sut.Build());
        }
    }
}
