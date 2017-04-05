// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequestWithResponse.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.UnitTests.Fakes
{
    using System.ComponentModel.DataAnnotations;

    using Collector.Common.RestClient.UnitTests.Client;
    using Collector.Common.RestContracts;

    public class RequestWithResponse : RequestBase<DummyResourceIdentifier, string>
    {
        public RequestWithResponse(DummyResourceIdentifier resourceIdentifier)
            : base(resourceIdentifier)
        {
        }

        [Required]
        public string StringProperty { get; set; }

        public override HttpMethod GetHttpMethod() => HttpMethod.POST;
    }

    public class RequestWithoutResponse : RequestBase<DummyResourceIdentifier>
    {
        public RequestWithoutResponse(DummyResourceIdentifier resourceIdentifier)
            : base(resourceIdentifier)
        {
        }

        [Required]
        public string StringProperty { get; set; } = "StringValue";

        public override HttpMethod GetHttpMethod() => HttpMethod.POST;
    }

    public class DummyResourceIdentifier : ResourceIdentifier
    {
        public override string Uri => "api.url.com";
    }
}