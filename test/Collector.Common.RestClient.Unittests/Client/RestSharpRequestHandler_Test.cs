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

    using NUnit.Framework;

    using Ploeh.AutoFixture;

    using RestSharp;

    using Rhino.Mocks;

    [TestFixture]
    public class RestSharpRequestHandler_Test
    {
        private RestSharpRequestHandler _sut;
        private Fixture _fixture;
        private RestSharpClientWrapper_Fake _restClientWrapper;
        private IRestClient _restClient;
        private IRestRequest _restRequest;

        [SetUp]
        protected void TestInitialize()
        {
            _fixture = new Fixture();
            _restClient = new RestClient_Fake
                              {
                                  BaseUrl = _fixture.Create<Uri>()
                              };
            _restRequest = new RestRequest_Fake();
            _restClientWrapper = new RestSharpClientWrapper_Fake();

            _sut = new RestSharpRequestHandler(_restClientWrapper);
        }

        [Test]
        public void When_authentication_is_provided_it_will_get_authorization_header()
        {
            var authorizationHeaderFactory = MockRepository.GenerateStub<IAuthorizationHeaderFactory>();

            _restClientWrapper.Authenticator = new RestSharpAuthenticator(authorizationHeaderFactory);

            _restClientWrapper.Authenticator.Authenticate(_restClient, _restRequest);

            authorizationHeaderFactory.AssertWasCalled(x => x.Get(Arg<RestAuthorizeRequestData>.Is.Anything));
        }

        [Test]
        public async void When_executing_call_async_the_rest_request_has_the_expected_uri()
        {
            var request = new RequestWithResponse(new DummyResourceIdentifier()) { StringProperty = _fixture.Create<string>() };
         

            await _sut.CallAsync(request);

            var restRequest = _restClientWrapper.LastRequest;

            Assert.AreEqual(request.GetResourceIdentifier().Uri, restRequest.Resource);
        }

        [Test]
        public async void When_executing_call_async_the_rest_request_has_the_expected_http_method()
        {
            var request = new RequestWithResponse(new DummyResourceIdentifier()) { StringProperty = _fixture.Create<string>() };

            await _sut.CallAsync(request);

            var restRequest = _restClientWrapper.LastRequest;

            Assert.AreEqual(Method.POST, restRequest.Method);
        }

        [Test]
        public async void When_executing_call_async_the_rest_request_without_response_has_the_expected_uri()
        {
            var request = new RequestWithoutResponse(new DummyResourceIdentifier()) { StringProperty = _fixture.Create<string>() };

            await _sut.CallAsync(request);

            var restRequest = _restClientWrapper.LastRequest;

            Assert.AreEqual(request.GetResourceIdentifier().Uri, restRequest.Resource);
        }

        [Test]
        public async void When_executing_call_async_the_rest_request_without_response_has_the_expected_http_method()
        {
            var request = new RequestWithoutResponse(new DummyResourceIdentifier()) { StringProperty = _fixture.Create<string>() };

            await _sut.CallAsync(request);

            var restRequest = _restClientWrapper.LastRequest;

            Assert.AreEqual(Method.POST, restRequest.Method);
        }

        [Test]
        public void When_executing_call_async_and_the_response_has_does_not_indicate_2xx_status_code_it_throws_an_exception()
        {
            var request = new RequestWithoutResponse(new DummyResourceIdentifier()) { StringProperty = _fixture.Create<string>() };
            _restClientWrapper.ExpectedError = null;
            _restClientWrapper.ExpectedResponseStatusCode = HttpStatusCode.BadRequest;

            var exception = Assert.Throws<RestClientCallException>(async () => await _sut.CallAsync(request));

            Assert.AreEqual(_restClientWrapper.ExpectedResponseStatusCode, exception.HttpStatusCode);
        }

        [Test]
        public void When_executing_call_async_and_the_response_has_an_error_it_throws_an_exception()
        {
            var request = new RequestWithoutResponse(new DummyResourceIdentifier()) { StringProperty = _fixture.Create<string>() };

            ConfigureRestSharpFakeWrapper();

            Assert.Throws<RestClientCallException>(async () => await _sut.CallAsync(request));
        }

        [Test]
        public void When_executing_call_async_and_the_response_has_an_error_it_throws_an_exception_with_the_correct_error()
        {
            var request = new RequestWithoutResponse(new DummyResourceIdentifier()) { StringProperty = _fixture.Create<string>() };
            var expectedError = _fixture.Create<Error>();

            ConfigureRestSharpFakeWrapper(expectedError);

            var exception = Assert.Throws<RestClientCallException>(async () => await _sut.CallAsync(request));

            Assert.AreEqual(expectedError.Code, exception.Error.Code);
            Assert.AreEqual(expectedError.Message, exception.Error.Message);
            CollectionAssert.AreEqual(expectedError.Errors.Select(e => e.Message), exception.Error.Errors.Select(e => e.Message));
            CollectionAssert.AreEqual(expectedError.Errors.Select(e => e.Reason), exception.Error.Errors.Select(e => e.Reason));
        }

        private void ConfigureRestSharpFakeWrapper(Error error = null, IEnumerable<ErrorInfo> errorInfos = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            var expectedError = error ?? _fixture.Create<Error>();
            var expectedErrorInfos = errorInfos ?? _fixture.CreateMany<ErrorInfo>();
            expectedError.Errors = expectedErrorInfos;

            _restClientWrapper.ExpectedError = expectedError;
            _restClientWrapper.ExpectedResponseStatusCode = statusCode;
        }
    }
}