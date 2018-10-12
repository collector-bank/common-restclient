namespace ContractsExamples.github
{
    using Collector.Common.RestContracts;
    using Collector.Common.RestContracts.Interfaces;

    using Newtonsoft.Json;

    public abstract class GithubApiBaseRequest<TResourceIdentifier, TResponse> : RequestBase<TResourceIdentifier, TResponse>
        where TResourceIdentifier : class, IResourceIdentifier where TResponse : class
    {
        protected GithubApiBaseRequest(TResourceIdentifier resourceIdentifier, string performedBy)
            : base(resourceIdentifier)
        {
        }

        public override string GetConfigurationKey()
        {
            return "Github";
        }

        protected TResponseType ParseResponse<TResponseType>(string content)
        {
            var settings = new JsonSerializerSettings
                           {
                               DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                               DateParseHandling = DateParseHandling.DateTimeOffset
                           };
            return JsonConvert.DeserializeObject<TResponseType>(content, settings);
        }
    }
}