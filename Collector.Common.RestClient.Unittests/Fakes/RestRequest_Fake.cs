// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestRequest_Fake.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient.Unittests.Fakes
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;

    using RestSharp;
    using RestSharp.Serializers;

    public class RestRequest_Fake : IRestRequest
    {
        public RestRequest_Fake()
        {
        }

        public RestRequest_Fake(string uri, string body)
        {
            Resource = uri;

            AddParameter("application/json", body, ParameterType.RequestBody);
        }

        public bool AlwaysMultipartFormData { get; set; }

        public ISerializer JsonSerializer { get; set; }

        public ISerializer XmlSerializer { get; set; }

        public Action<Stream> ResponseWriter { get; set; }

        public List<Parameter> Parameters { get; } = new List<Parameter>();

        public List<FileParameter> Files { get; } = new List<FileParameter>();

        public Method Method { get; set; }

        public string Resource { get; set; }

        public DataFormat RequestFormat { get; set; }

        public string RootElement { get; set; }

        public string DateFormat { get; set; }

        public string XmlNamespace { get; set; }

        public ICredentials Credentials { get; set; }

        public int Timeout { get; set; }

        public int ReadWriteTimeout { get; set; }

        public int Attempts { get; } = 0;

        public bool UseDefaultCredentials { get; set; }

        public Action<IRestResponse> OnBeforeDeserialization { get; set; }

        public IRestRequest AddFile(string name, string path, string contentType = null)
        {
            throw new NotImplementedException();
        }

        public IRestRequest AddFile(string name, byte[] bytes, string fileName, string contentType = null)
        {
            throw new NotImplementedException();
        }

        public IRestRequest AddFile(string name, Action<Stream> writer, string fileName, string contentType = null)
        {
            throw new NotImplementedException();
        }

        public IRestRequest AddFileBytes(string name, byte[] bytes, string filename, string contentType = "application/x-gzip")
        {
            throw new NotImplementedException();
        }

        public IRestRequest AddBody(object obj, string xmlNamespace)
        {
            throw new NotImplementedException();
        }

        public IRestRequest AddBody(object obj)
        {
            throw new NotImplementedException();
        }

        public IRestRequest AddJsonBody(object obj)
        {
            throw new NotImplementedException();
        }

        public IRestRequest AddXmlBody(object obj)
        {
            throw new NotImplementedException();
        }

        public IRestRequest AddXmlBody(object obj, string xmlNamespace)
        {
            throw new NotImplementedException();
        }

        public IRestRequest AddObject(object obj, params string[] includedProperties)
        {
            throw new NotImplementedException();
        }

        public IRestRequest AddObject(object obj)
        {
            throw new NotImplementedException();
        }

        public IRestRequest AddParameter(Parameter p)
        {
            throw new NotImplementedException();
        }

        public IRestRequest AddParameter(string name, object value)
        {
            throw new NotImplementedException();
        }

        public IRestRequest AddParameter(string name, object value, ParameterType type)
        {
            Parameters.Add(new Parameter() { Name = name, ContentType = "application/json", Value = value, Type = type });
            return this;
        }

        public IRestRequest AddParameter(string name, object value, string contentType, ParameterType type)
        {
            throw new NotImplementedException();
        }

        public IRestRequest AddHeader(string name, string value)
        {
            throw new NotImplementedException();
        }

        public IRestRequest AddCookie(string name, string value)
        {
            throw new NotImplementedException();
        }

        public IRestRequest AddUrlSegment(string name, string value)
        {
            throw new NotImplementedException();
        }

        public IRestRequest AddQueryParameter(string name, string value)
        {
            throw new NotImplementedException();
        }

        public void IncreaseNumAttempts()
        {
            throw new NotImplementedException();
        }
    }
}