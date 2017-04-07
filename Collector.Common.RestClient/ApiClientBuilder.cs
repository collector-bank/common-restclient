// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiClientBuilder.cs" company="Collector AB">
//   Copyright © Collector AB. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Collector.Common.RestClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Collector.Common.RestClient.Exceptions;
    using Collector.Common.RestClient.Implementation;
    using Collector.Common.RestClient.Interfaces;

    using Serilog;

    public class ApiClientBuilder : IApiClientBuilder
    {
        internal readonly IDictionary<string, string> BaseUris = new Dictionary<string, string>();

        internal readonly IDictionary<string, IAuthorizationHeaderFactory> Authenticators = new Dictionary<string, IAuthorizationHeaderFactory>();

        private ILogger _logger;

        private IRequestHandler _requestHandler;

        public IApiClientBuilder ConfigureContractByKey(string contractKey, string baseUrl, IAuthorizationHeaderFactory authorizationHeaderFactory = null)
        {
            if (string.IsNullOrEmpty(contractKey))
                throw new ArgumentNullException(nameof(contractKey));

            if (BaseUris.ContainsKey(contractKey))
            {
                throw new BuildException($"{contractKey} has already been configured.");
            }

            BaseUris.Add(contractKey, baseUrl);

            if (authorizationHeaderFactory != null)
            {
                Authenticators.Add(contractKey, authorizationHeaderFactory);
            }

            return this;
        }

        public IApiClientBuilder ConfigureLogging(ILogger logger)
        {
            _logger = logger;
            return this;
        }

        public IApiClientBuilder UseRestSharp()
        {
            var wrapper = new RestSharpClientWrapper(BaseUris, Authenticators);

            _requestHandler = new RestSharpRequestHandler(wrapper);

            return this;
        }

        public IRestApiClient Build()
        {
            if (!BaseUris.Any())
            {
                throw new BuildException("Please configure atleast one base uri");
            }

            if (_requestHandler == null)
            {
                throw new BuildException("Please choose a Rest client type.");
            }

            return new RestApiClient(_requestHandler, _logger);
        }
    }
}