// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestSharpRequestHandler_Test.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.UnitTests.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    using Collector.Common.RestClient.Authorization;
    using Collector.Common.RestClient.Exceptions;
    using Collector.Common.RestClient.RestSharpClient;
    using Collector.Common.RestClient.UnitTests.Fakes;
    using Collector.Common.RestContracts;
    using Collector.Common.UnitTest.Helpers;
    using Collector.Common.UnitTest.Helpers.Autofixture;

    using NUnit.Framework;

    using Ploeh.AutoFixture;

    using RestSharp;

    using Rhino.Mocks;

    public class RestSharpRequestHandler_Test : BaseUnitTest<CommonFixture>
    {
        private RestSharpRequestHandler _sut;

        protected override void OnTestInitialize()
        {
            Fixture.Inject<IRestClient>(new RestClient_Fake
            {
                BaseUrl = Fixture.Create<Uri>()
            });
            Fixture.Inject<IRestRequest>(new RestRequest_Fake());
            Fixture.Inject<IRestSharpClientWrapper>(new RestSharpClientWrapper_Fake());

            _sut = new RestSharpRequestHandler(Fixture.Create<IRestSharpClientWrapper>());
        }

        [Test]
        public void When_authentication_is_provided_it_will_get_authorization_header()
        {
            var authorizationHeaderFactory = Fixture.Freeze<IAuthorizationHeaderFactory>();
            var restClientWrapper = (RestSharpClientWrapper_Fake)Fixture.Create<IRestSharpClientWrapper>();

            restClientWrapper.Authenticator = new RestSharpAuthenticator(authorizationHeaderFactory);

            restClientWrapper.Authenticator.Authenticate(Fixture.Create<IRestClient>(), Fixture.Create<IRestRequest>());

            authorizationHeaderFactory.AssertWasCalled(x => x.Get(Arg<RestAuthorizeRequestData>.Is.Anything));
        }

        [Test]
        public async void When_executing_call_async_the_rest_request_has_the_expected_uri()
        {
            var request = new RequestWithResponse(new DummyResourceIdentifier()) { StringProperty = Fixture.Create<string>() };
            var restClientWrapper = (RestSharpClientWrapper_Fake)Fixture.Create<IRestSharpClientWrapper>();

            await _sut.CallAsync(request);

            var restRequest = restClientWrapper.LastRequest;

            Assert.AreEqual(request.GetResourceIdentifier().Uri, restRequest.Resource);
        }

        [Test]
        public async void When_executing_call_async_the_rest_request_has_the_expected_http_method()
        {
            var request = new RequestWithResponse(new DummyResourceIdentifier()) { StringProperty = Fixture.Create<string>() };
            var restClientWrapper = (RestSharpClientWrapper_Fake)Fixture.Create<IRestSharpClientWrapper>();

            await _sut.CallAsync(request);

            var restRequest = restClientWrapper.LastRequest;

            Assert.AreEqual(Method.POST, restRequest.Method);
        }

        [Test]
        public async void When_executing_call_async_the_rest_request_without_response_has_the_expected_uri()
        {
            var request = new RequestWithoutResponse(new DummyResourceIdentifier()) { StringProperty = Fixture.Create<string>() };
            var restClientWrapper = (RestSharpClientWrapper_Fake)Fixture.Create<IRestSharpClientWrapper>();

            await _sut.CallAsync(request);

            var restRequest = restClientWrapper.LastRequest;

            Assert.AreEqual(request.GetResourceIdentifier().Uri, restRequest.Resource);
        }

        [Test]
        public async void When_executing_call_async_the_rest_request_without_response_has_the_expected_http_method()
        {
            var request = new RequestWithoutResponse(new DummyResourceIdentifier()) { StringProperty = Fixture.Create<string>() };
            var restClientWrapper = (RestSharpClientWrapper_Fake)Fixture.Create<IRestSharpClientWrapper>();

            await _sut.CallAsync(request);

            var restRequest = restClientWrapper.LastRequest;

            Assert.AreEqual(Method.POST, restRequest.Method);
        }

        [Test]
        public void When_executing_call_async_and_the_response_has_does_not_indicate_2xx_status_code_it_throws_an_exception()
        {
            var request = new RequestWithoutResponse(new DummyResourceIdentifier()) { StringProperty = Fixture.Create<string>() };
            var restClientWrapper = (RestSharpClientWrapper_Fake)Fixture.Create<IRestSharpClientWrapper>();
            restClientWrapper.ExpectedError = null;
            restClientWrapper.ExpectedResponseStatusCode = HttpStatusCode.BadRequest;
            
            var exception = Assert.Throws<RestClientCallException>(async () => await _sut.CallAsync(request));

            Assert.AreEqual(restClientWrapper.ExpectedResponseStatusCode, exception.HttpStatusCode);
        }

        [Test]
        public void When_executing_call_async_and_the_response_has_an_error_it_throws_an_exception()
        {
            var request = new RequestWithoutResponse(new DummyResourceIdentifier()) { StringProperty = Fixture.Create<string>() };

            ConfigureRestSharpFakeWrapper();

            Assert.Throws<RestClientCallException>(async () => await _sut.CallAsync(request));
        }

        [Test]
        public void When_executing_call_async_and_the_response_has_an_error_it_throws_an_exception_with_the_correct_error()
        {
            var request = new RequestWithoutResponse(new DummyResourceIdentifier()) { StringProperty = Fixture.Create<string>() };
            var expectedError = Fixture.Create<Error>();

            ConfigureRestSharpFakeWrapper(expectedError);

            var exception = Assert.Throws<RestClientCallException>(async () => await _sut.CallAsync(request));

            Assert.AreEqual(expectedError.Code, exception.Error.Code);
            Assert.AreEqual(expectedError.Message, exception.Error.Message);
            CollectionAssert.AreEqual(expectedError.Errors.Select(e => e.Message), exception.Error.Errors.Select(e => e.Message));
            CollectionAssert.AreEqual(expectedError.Errors.Select(e => e.Reason), exception.Error.Errors.Select(e => e.Reason));
        }

        private void ConfigureRestSharpFakeWrapper(Error error = null, IEnumerable<ErrorInfo> errorInfos = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            var expectedError = error ?? Fixture.Create<Error>();
            var expectedErrorInfos = errorInfos ?? Fixture.CreateMany<ErrorInfo>();
            expectedError.Errors = expectedErrorInfos;

            var restClientWrapper = (RestSharpClientWrapper_Fake)Fixture.Create<IRestSharpClientWrapper>();
            restClientWrapper.ExpectedError = expectedError;
            restClientWrapper.ExpectedResponseStatusCode = statusCode;
        }
    }
}