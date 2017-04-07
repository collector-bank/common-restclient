// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestApiClient_Test.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.UnitTests.Client
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;

    using Collector.Common.RestClient.Implementation;
    using Collector.Common.RestClient.Interfaces;
    using Collector.Common.RestClient.UnitTests.Fakes;
    using Collector.Common.UnitTest.Helpers;
    using Collector.Common.UnitTest.Helpers.Autofixture;
    using Collector.Common.UnitTest.Helpers.Extensions;

    using NUnit.Framework;

    using Ploeh.AutoFixture;

    using Rhino.Mocks;

    using Serilog;
    using Serilog.Events;

    [TestFixture]
    public class RestApiClient_Test : BaseUnitTest<CommonFixture>
    {
        private IList<LogEvent> _logEvents;

        private RestApiClient _sut;
        private IRequestHandler _stub;

        protected override void OnTestInitialize()
        {
            _stub = Fixture.GetStub<IRequestHandler>();

            _stub.Stub(x => x.CallAsync(Arg<RequestWithResponse>.Is.Anything)).Return(Task.FromResult(Fixture.Create<string>()));
            _stub.Stub(x => x.CallAsync(Arg<RequestWithoutResponse>.Is.Anything)).Return(Task.FromResult(Fixture.Create<string>()));


            _logEvents = new List<LogEvent>();
            _sut = new RestApiClient(_stub, new LoggerConfiguration().WriteTo.Sink(new DelegatingSink(_logEvents.Add)).CreateLogger());
        }

        [Test, ExpectedException(typeof(ValidationException))]
        public async void When_executing_call_async_and_the_request_is_not_valid_it_throws_an_exception()
        {
            var request = new RequestWithResponse(new DummyResourceIdentifier());

            await _sut.CallAsync(request);
        }

        [Test, ExpectedException(typeof(ValidationException))]
        public async void When_executing_call_async_and_the_request_without_response_is_not_valid_it_throws_an_exception()
        {
            var request = new RequestWithoutResponse(new DummyResourceIdentifier()) { StringProperty = null };

            await _sut.CallAsync(request);
        }

        [Test]
        public async void When_executing_call_async_and_the_request_with_response_it_will_log_it()
        {
            var request = new RequestWithResponse(new DummyResourceIdentifier())
            {
                StringProperty = Fixture.Create<string>()
            };

            await _sut.CallAsync(request);

            var anyLogEvents = _logEvents.Count(x => x.Level == LogEventLevel.Information);

            Assert.AreEqual(2, anyLogEvents);
        }

        [Test]
        public async void When_executing_call_async_and_the_request_without_response_it_will_log_it()
        {
            var request = new RequestWithoutResponse(new DummyResourceIdentifier())
            {
                StringProperty = Fixture.Create<string>()
            };

            await _sut.CallAsync(request);

            var anyLogEvents = _logEvents.Count(x => x.Level == LogEventLevel.Information);

            Assert.AreEqual(1, anyLogEvents);
        }

        protected override void OnTestFinalize()
        {
            _logEvents.Clear();
        }
    }
}