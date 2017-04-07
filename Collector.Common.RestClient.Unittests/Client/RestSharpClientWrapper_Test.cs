// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestSharpClientWrapper_Test.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.UnitTests.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Collector.Common.RestClient.Implementation;
    using Collector.Common.RestClient.Interfaces;
    using Collector.Common.UnitTest.Helpers;
    using Collector.Common.UnitTest.Helpers.Autofixture;

    using NUnit.Framework;

    using Ploeh.AutoFixture;

    using RestSharp;

    using Rhino.Mocks;

    [TestFixture]
    public class RestSharpClientWrapper_Test : BaseUnitTest<CommonFixture>
    {
        private IDictionary<string, string> _contractBaseUrlMappings;
        private IDictionary<string, IAuthorizationHeaderFactory> _authorizationHeaderFactories;

        private RestSharpClientWrapper _sut;

        protected override void OnTestInitialize()
        {
            var baseUrl = Fixture.Create<Uri>();
            var contract = Fixture.Create<string>();

            _contractBaseUrlMappings = new Dictionary<string, string> { { contract, baseUrl.ToString() } };
            _authorizationHeaderFactories = new Dictionary<string, IAuthorizationHeaderFactory> { { contract, Fixture.Create<IAuthorizationHeaderFactory>() } };

            _sut = new RestSharpClientWrapper(_contractBaseUrlMappings, _authorizationHeaderFactories);
        }

        [Test]
        public void When_configuring_with_base_url_it_will_build_a_client_with_correct_base_url()
        {
            var baseUrl = _contractBaseUrlMappings.First();

            var builtClient = _sut.RestClients[baseUrl.Key];

            Assert.AreEqual(baseUrl.Value, builtClient.BaseUrl.ToString());
        }

        [Test]
        public void When_configuring_with_base_url_it_will_build_a_client_with_correct_authorization()
        {
            var baseUrl = _contractBaseUrlMappings.First();
            var authenticator = _authorizationHeaderFactories[baseUrl.Key];

            var builtClient = _sut.RestClients[baseUrl.Key];

            Assert.AreEqual(authenticator, ((RestSharpAuthenticator)builtClient.Authenticator).AuthorizationHeaderFactory);
        }

        [Test]
        public void When_making_a_request_and_the_configured_contract_does_not_exist_exception_is_thrown()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                                                       {
                                                           _sut.ExecuteAsync(null, Fixture.Create<string>(), null);
                                                       });
        }

        [Test]
        public void When_making_a_request_and_the_configured_contract_exists_it_will_call_the_rest_client()
        {
            var restClient = Fixture.Create<IRestClient>();
            var contract = Fixture.Create<string>();

            _sut.RestClients.Add(new KeyValuePair<string, IRestClient>(contract, restClient));

            _sut.ExecuteAsync(null, contract, null);

            restClient.AssertWasCalled(x =>
            x.ExecuteAsync(Arg<IRestRequest>.Is.Anything, Arg<Action<IRestResponse>>.Is.Anything));
        }
    }
}