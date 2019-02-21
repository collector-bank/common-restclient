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

    public class GetRequestWithResponse : RequestBase<DummyResourceIdentifier, string>
    {
        public GetRequestWithResponse(DummyResourceIdentifier resourceIdentifier)
            : base(resourceIdentifier)
        {
        }

        public decimal DecimalProperty { get; set; } = 3.14159265359m;

        public double DoubleProperty { get; set; } = 2.7182818284;

        public float FloatProperty { get; set; } = 1.41421f;

        public override HttpMethod GetHttpMethod() => HttpMethod.GET;

        public override string GetConfigurationKey()
        {
            return "Test";
        }
    }

    public class DummyResourceIdentifier : ResourceIdentifier
    {
        public override string Uri => "api.url.com";
    }
}