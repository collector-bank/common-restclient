namespace Collector.Common.RestClient.UnitTests.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Collector.Common.RestClient.Exceptions;
    using Collector.Common.RestClient.UnitTests.Fakes;

    using NUnit.Framework;

    using AutoFixture;

    using Collector.Common.RestContracts;

    using Moq;

    [TestFixture]
    public class RestApiClient_Test
    {
        private RestApiClient _sut;
        private Mock<IRequestHandler> _stub;
        private string _context;
        private Fixture _fixture;
        private IReadOnlyDictionary<string, string> _headers;

        [SetUp]
        protected void TestInitialize()
        {
            _fixture = new Fixture();
            _stub = new Mock<IRequestHandler>();
            _stub.Setup(x => x.CallAsync(It.IsAny<RequestWithResponse>())).Returns(Task.FromResult(_fixture.Create<string>()));

            _context = _fixture.Create<string>();
            _headers = _fixture.CreateMany<KeyValuePair<string, string>>(2).ToDictionary(x => x.Key, x => x.Value);
            _sut = new RestApiClient(_stub.Object, () => _context, headersFunc: () => _headers);
        }

        [Test]
        public void When_executing_call_async_and_the_request_is_not_valid_it_throws_an_exception()
        {
            var request = new RequestWithResponse(new DummyResourceIdentifier());

            Assert.ThrowsAsync<RestClientCallException>(async () => await _sut.CallAsync(request));
        }

        [Test]
        public void When_executing_call_async_and_the_request_without_response_is_not_valid_it_throws_an_exception()
        {
            var request = new RequestWithoutResponse(new DummyResourceIdentifier()) { StringProperty = null };

            Assert.ThrowsAsync<RestClientCallException>(async () => await _sut.CallAsync(request));
        }

        [Test]
        public async Task When_executing_call_async_and_the_request_does_not_have_a_context_then_a_context_is_added()
        {
            var request = new RequestWithoutResponse(new DummyResourceIdentifier())
            {
                StringProperty = _fixture.Create<string>()
            };

            await _sut.CallAsync(request);

            Assert.AreEqual(_context, request.Context);
        }

        [Test]
        public async Task When_executing_call_then_headers_are_set_on_request()
        {
            var request = new RequestWithoutResponse(new DummyResourceIdentifier()) { StringProperty = _fixture.Create<string>() };

            await _sut.CallAsync(request);

            CollectionAssert.AreEquivalent(_headers, request.GetHeaders());
        }

        [Test]
        public async Task When_executing_call_with_response_then_headers_are_set_on_request()
        {
            var request = new RequestWithResponse(new DummyResourceIdentifier()) { StringProperty = _fixture.Create<string>() };

            await _sut.CallAsync(request);

            CollectionAssert.AreEquivalent(_headers, request.GetHeaders());
        }
    }

    [TestFixture]
    public class RestApiClientWithResilienceHandler_Tests
    {
        private Fixture _fixture;
        private Mock<IRequestHandler> _stub;
        private string _context;
        private Mock<IResilienceHandler> _resilienceHandler;
        private RestApiClient _sut;

        private string _expectedValue;
        
        [SetUp]
        protected void TestInitialize()
        {
            _fixture = new Fixture();
            _stub = new Mock<IRequestHandler>();

            _expectedValue = _fixture.Create<string>();

            _context = _fixture.Create<string>();
            _resilienceHandler = new Mock<IResilienceHandler>();

            _resilienceHandler.Setup(x => x.ExecuteAsync(It.IsAny<RequestWithoutResponse>(), It.IsAny<Func<RequestBase<DummyResourceIdentifier>, Task>>())).Returns(Task.FromResult(0)).Verifiable();
            _resilienceHandler.Setup(x => x.ExecuteAsync(It.IsAny<RequestWithResponse>(), It.IsAny<Func<RequestBase<DummyResourceIdentifier, string>, Task<string>>>())).Returns(Task.FromResult(_expectedValue));

            _sut = new RestApiClient(_stub.Object, () => _context, _resilienceHandler.Object);
        }

        [Test]
        public async Task When_executing_call_async_with_resilience_handler_and_request_without_response_then_verify_resilience_handler_is_called()
        {
            var request = new RequestWithoutResponse(new DummyResourceIdentifier())
                          {
                              StringProperty = _fixture.Create<string>()
                          };

            await _sut.CallAsync(request);

            _resilienceHandler.Verify();
        }

        [Test]
        public async Task When_executing_call_async_with_resilience_handler_and_request_with_response_then_verify_resilience_handler_is_called()
        {
            var request = new RequestWithResponse(new DummyResourceIdentifier())
                          {
                              StringProperty = _fixture.Create<string>()
                          };

            var result = await _sut.CallAsync(request);

            Assert.AreEqual(_expectedValue, result);
        }
    }
}