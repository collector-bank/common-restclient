// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestAuthorizeRequestData.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Authorization
{
    using System;
    
    using Collector.Common.RestContracts;

    using RestSharp;

    public class RestAuthorizeRequestData
    {
        public RestAuthorizeRequestData(string body, Uri uri, Method method)
            : this(body, uri, (HttpMethod)Enum.Parse(typeof(HttpMethod), method.ToString()))
        {
        }

        public RestAuthorizeRequestData(string body, Uri uri, HttpMethod method)
        {
            Body = body;
            Uri = uri;
            HttpMethod = method;
        }

        public string Body { get; }

        public Uri Uri { get; }

        public HttpMethod HttpMethod { get; }
    }
}