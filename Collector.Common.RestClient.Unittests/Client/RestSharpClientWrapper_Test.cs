// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestSharpClientWrapper_Test.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.UnitTests.Client
{
    using System;
    using System.Collections.Generic;

    using Collector.Common.RestClient.Exceptions;
    using Collector.Common.RestClient.Implementation;
    using Collector.Common.RestClient.Interfaces;
    using Collector.Common.UnitTest.Helpers;
    using Collector.Common.UnitTest.Helpers.Autofixture;

    using NUnit.Framework;

    using Ploeh.AutoFixture;

    [TestFixture]
    public class RestSharpClientWrapper_Test : BaseUnitTest<CommonFixture>
    {
        private RestSharpClientWrapper _sut;
        private Uri _baseUrl;
        private string _contract;
        private IAuthorizationHeaderFactory _authorizationHeaderFactory;

        protected override void OnTestInitialize()
        {
            _baseUrl = Fixture.Create<Uri>();
            _contract = Fixture.Create<string>();
            _authorizationHeaderFactory = Fixture.Create<IAuthorizationHeaderFactory>();

            var contractBaseUrlMappings = new Dictionary<string, string> { { _contract, _baseUrl.ToString() } };
            var authorizationHeaderFactories = new Dictionary<string, IAuthorizationHeaderFactory> { { _contract, _authorizationHeaderFactory } };

            _sut = new RestSharpClientWrapper(contractBaseUrlMappings, authorizationHeaderFactories);
        }

        [Test]
        public void When_configuring_with_base_url_it_will_build_a_client_with_correct_base_url()
        {
            _sut.InitRestClient(_contract);

            var builtClient = _sut.RestClients[_contract];

            Assert.AreEqual(_baseUrl, builtClient.BaseUrl.ToString());
        }

        [Test]
        public void When_configuring_with_base_url_it_will_build_a_client_with_correct_authorization()
        {
            _sut.InitRestClient(_contract);

            var builtClient = _sut.RestClients[_contract];

            Assert.AreSame(_authorizationHeaderFactory, ((RestSharpAuthenticator)builtClient.Authenticator).AuthorizationHeaderFactory);
        }

        [Test]
        public void When_making_a_request_and_the_configured_contract_does_not_exist_exception_is_thrown()
        {
            Assert.Throws<BuildException>(() =>
                                                       {
                                                           _sut.ExecuteAsync(null, Fixture.Create<string>(), null);
                                                       });
        }
    }
}