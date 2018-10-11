namespace Collector.Common.RestClient.UnitTests.Fakes
{
    using System.Collections.Generic;
    using System.Linq;
    
    using Collector.Common.RestContracts;

    public class RequestWithResponse : RequestBase<DummyResourceIdentifier, string>
    {
        public RequestWithResponse(DummyResourceIdentifier resourceIdentifier)
            : base(resourceIdentifier)
        {
        }
        
        public string StringProperty { get; set; }

        public override HttpMethod GetHttpMethod() => HttpMethod.POST;

        public override string GetConfigurationKey()
        {
            return "Test";
        }

        protected override IEnumerable<string> ValidateRequest()
        {
            if (StringProperty == null)
                return new []{ "String property is required"};

            return Enumerable.Empty<string>();
        }
    }

    public class RequestWithoutResponse : RequestBase<DummyResourceIdentifier>
    {
        public RequestWithoutResponse(DummyResourceIdentifier resourceIdentifier)
            : base(resourceIdentifier)
        {
        }
        
        public string StringProperty { get; set; } = "StringValue";

        public override HttpMethod GetHttpMethod() => HttpMethod.POST;

        public override string GetConfigurationKey()
        {
            return "Test";
        }

        protected override IEnumerable<string> ValidateRequest()
        {
            if (StringProperty == null)
                return new[] { "String property is required" };

            return Enumerable.Empty<string>();
        }
    }

    public class GetRequestWithoutResponse : RequestBase<DummyResourceIdentifier>
    {
        public GetRequestWithoutResponse(DummyResourceIdentifier resourceIdentifier)
            : base(resourceIdentifier)
        {
        }

        public string[] ArrayProperty { get; set; }

        public override HttpMethod GetHttpMethod() => HttpMethod.GET;

        public override string GetConfigurationKey()
        {
            return "Test";
        }

        protected override IEnumerable<string> ValidateRequest()
        {
            if (ArrayProperty == null)
                return new[] { "Array property is required" };

            return Enumerable.Empty<string>();
        }
    }

    public class DummyResourceIdentifier : ResourceIdentifier
    {
        public override string Uri => "api.url.com";
    }
}