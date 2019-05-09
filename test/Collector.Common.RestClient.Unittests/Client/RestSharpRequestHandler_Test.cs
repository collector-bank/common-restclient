namespace Collector.Common.RestClient.UnitTests.Client
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using Collector.Common.RestClient.Authorization;
    using Collector.Common.RestClient.Exceptions;
    using Collector.Common.RestClient.RestSharpClient;
    using Collector.Common.RestClient.UnitTests.Fakes;
    using Collector.Common.RestContracts;

    using NUnit.Framework;

    using AutoFixture;

    using RestSharp;

    using Moq;

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
        public async Task When_get_request_has_enumerable_the_values_are_set_in_query_string()
        {
            var request = new GetRequestEnumerable(new DummyResourceIdentifier());
            
            await _sut.CallAsync(request);

            var restRequest = _restClientWrapper.LastRequest;

            Assert.AreEqual("StringProperty", restRequest.Parameters[0].Name);
            Assert.AreEqual("StringVal", restRequest.Parameters[0].Value);
            Assert.AreEqual("DecimalProperty", restRequest.Parameters[1].Name);
            Assert.AreEqual("3.14159265359", restRequest.Parameters[1].Value);

            Assert.AreEqual("TestEnumProperty", restRequest.Parameters[2].Name);
            Assert.AreEqual(TestEnum.Foo, restRequest.Parameters[2].Value);

            Assert.AreEqual("EnumerableStringProperty", restRequest.Parameters[3].Name);
            Assert.AreEqual("test1", restRequest.Parameters[3].Value);
            Assert.AreEqual("EnumerableStringProperty", restRequest.Parameters[4].Name);
            Assert.AreEqual("test2", restRequest.Parameters[4].Value);
            Assert.AreEqual("EnumerableStringProperty", restRequest.Parameters[5].Name);
            Assert.AreEqual("test3", restRequest.Parameters[5].Value);
            
            Assert.AreEqual("EnumerableEnumProperty", restRequest.Parameters[6].Name);
            Assert.AreEqual(TestEnum.Foo, restRequest.Parameters[6].Value);
            Assert.AreEqual("EnumerableEnumProperty", restRequest.Parameters[7].Name);
            Assert.AreEqual(TestEnum.Baar, restRequest.Parameters[7].Value);
        }

        [Test]
        public async Task When_get_request_has_no_properties_no_values_are_set_in_query_string()
        {
            var request = new GetRequestWithoutPropertiesWithoutResponse(new DummyResourceIdentifier());
            
            await _sut.CallAsync(request);

            var restRequest = _restClientWrapper.LastRequest;

            Assert.IsEmpty(restRequest.Parameters);
        }

        [Test]
        public void When_authentication_is_provided_it_will_get_authorization_header()
        {
            var authorizationHeaderFactory = new Mock<IAuthorizationHeaderFactory>();

            _restClientWrapper.Authenticator = new RestSharpAuthenticator(authorizationHeaderFactory.Object);

            _restClientWrapper.Authenticator.Authenticate(_restClient, _restRequest);

            authorizationHeaderFactory.Verify(x => x.Get(It.IsAny<RestAuthorizeRequestData>()));
        }

        [Test]
        public async Task When_executing_call_async_the_rest_request_has_the_expected_uri()
        {
            var request = new RequestWithResponse(new DummyResourceIdentifier()) { StringProperty = _fixture.Create<string>() };
         

            await _sut.CallAsync(request);

            var restRequest = _restClientWrapper.LastRequest;

            Assert.AreEqual(request.GetResourceIdentifier().Uri, restRequest.Resource);
        }

        [Test]
        public async Task When_executing_call_async_the_rest_request_has_the_expected_http_method()
        {
            var request = new RequestWithResponse(new DummyResourceIdentifier()) { StringProperty = _fixture.Create<string>() };

            await _sut.CallAsync(request);

            var restRequest = _restClientWrapper.LastRequest;

            Assert.AreEqual(Method.POST, restRequest.Method);
        }

        [Test]
        public async Task When_executing_call_async_the_rest_request_without_response_has_the_expected_uri()
        {
            var request = new RequestWithoutResponse(new DummyResourceIdentifier()) { StringProperty = _fixture.Create<string>() };

            await _sut.CallAsync(request);

            var restRequest = _restClientWrapper.LastRequest;

            Assert.AreEqual(request.GetResourceIdentifier().Uri, restRequest.Resource);
        }

        [Test]
        public async Task When_executing_call_async_the_rest_request_without_response_has_the_expected_http_method()
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

            var exception = Assert.ThrowsAsync<RestClientCallException>(async () => await _sut.CallAsync(request));

            Assert.AreEqual(_restClientWrapper.ExpectedResponseStatusCode, exception.HttpStatusCode);
        }

        [Test]
        public void When_executing_call_async_and_the_response_has_an_error_it_throws_an_exception()
        {
            var request = new RequestWithoutResponse(new DummyResourceIdentifier()) { StringProperty = _fixture.Create<string>() };

            ConfigureRestSharpFakeWrapper();

            Assert.ThrowsAsync<RestClientCallException>(async () => await _sut.CallAsync(request));
        }

        [Test]
        public void When_executing_call_async_and_the_response_has_an_error_it_throws_an_exception_with_the_correct_error()
        {
            var request = new RequestWithoutResponse(new DummyResourceIdentifier()) { StringProperty = _fixture.Create<string>() };
            var expectedError = _fixture.Create<Error>();

            ConfigureRestSharpFakeWrapper(expectedError);

            var exception = Assert.ThrowsAsync<RestClientCallException>(async () => await _sut.CallAsync(request));

            Assert.AreEqual(expectedError.Code, exception.Error.Code);
            Assert.AreEqual(expectedError.Message, exception.Error.Message);
            CollectionAssert.AreEqual(expectedError.Errors.Select(e => e.Message), exception.Error.Errors.Select(e => e.Message));
            CollectionAssert.AreEqual(expectedError.Errors.Select(e => e.Reason), exception.Error.Errors.Select(e => e.Reason));
        }

        [Test]
        public async Task When_executing_call_async_the_decimal_property_is_converted_to_string_using_invariant_culture()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("sv-SE");
            var request = new GetRequestWithResponse(new DummyResourceIdentifier());

            await _sut.CallAsync(request);

            var restRequest = _restClientWrapper.LastRequest;
            var decimalPropertyParameter = restRequest.Parameters.First(p => p.Name == nameof(GetRequestWithResponse.DecimalProperty));

            Assert.AreEqual("3.14159265359", decimalPropertyParameter.Value);
        }

        [Test]
        public async Task When_executing_call_async_the_double_property_is_converted_to_string_using_invariant_culture()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("sv-SE");
            var request = new GetRequestWithResponse(new DummyResourceIdentifier());

            await _sut.CallAsync(request);

            var restRequest = _restClientWrapper.LastRequest;
            var doublePropertyParameter = restRequest.Parameters.First(p => p.Name == nameof(GetRequestWithResponse.DoubleProperty));

            Assert.AreEqual("2.7182818284", doublePropertyParameter.Value);
        }

        [Test]
        public async Task When_executing_call_async_the_float_property_is_converted_to_string_using_invariant_culture()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("sv-SE");
            var request = new GetRequestWithResponse(new DummyResourceIdentifier());

            await _sut.CallAsync(request);

            var restRequest = _restClientWrapper.LastRequest;
            var floatPropertyParameter = restRequest.Parameters.First(p => p.Name == nameof(GetRequestWithResponse.FloatProperty));

            Assert.AreEqual("1.41421", floatPropertyParameter.Value);
        }

        [Test]
        public async Task When_executing_call_async_and_the_request_has_headers_then_headers_are_added_to_rest_request()
        {
            var name = _fixture.Create<string>();
            var value = _fixture.Create<string>();
            var request = new RequestWithoutResponse(new DummyResourceIdentifier())
                          {
                              Headers = new Dictionary<string, string>
                                        {
                                            { name, value }
                                        }
                          };

            await _sut.CallAsync(request);

            var restRequest = (RestRequest)_restClientWrapper.LastRequest;

            var result = restRequest.Parameters.SingleOrDefault(p => p.Name == name && p.Type == ParameterType.HttpHeader);

            Assert.NotNull(result);
            Assert.AreEqual(value, result.Value);
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