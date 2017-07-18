namespace Collector.Common.RestClient.RestSharpClient
{
    using System.Linq;

    using Collector.Common.RestClient.Authorization;

    using RestSharp;
    using RestSharp.Authenticators;

    internal class RestSharpAuthenticator : IAuthenticator
    {
        internal readonly IAuthorizationHeaderFactory AuthorizationHeaderFactory;

        public RestSharpAuthenticator(IAuthorizationHeaderFactory authorizationHeaderFactory)
        {
            AuthorizationHeaderFactory = authorizationHeaderFactory;
        }

        public void Authenticate(IRestClient client, IRestRequest request)
        {
            var body = GetBody(request);

            var httpRequestData = new RestAuthorizeRequestData(body, client.BuildUri(request), request.Method);

            var authorizationHeaderValue = AuthorizationHeaderFactory.Get(httpRequestData);

            request.AddParameter("Authorization", authorizationHeaderValue, ParameterType.HttpHeader);
        }

        private static string GetBody(IRestRequest request)
        {
            var bodyParameter = request.Parameters.FirstOrDefault(p => p.Type == ParameterType.RequestBody);
            if (bodyParameter == null)
                return string.Empty;

            return bodyParameter.Value as string;
        }
    }
}