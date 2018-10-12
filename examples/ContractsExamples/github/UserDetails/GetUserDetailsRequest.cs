namespace ContractsExamples.github.UserDetails
{
    using Collector.Common.RestContracts;
    using Collector.Common.RestContracts.Interfaces;

    public class GetUserDetailsRequest : GithubApiBaseRequest<UserDetailsResourceIdentifier, UserDetailsResponse>,
        ISuccessfulResponseParser<UserDetailsResponse>
    {
        public GetUserDetailsRequest(string login, string performedBy)
            : base(new UserDetailsResourceIdentifier(login), performedBy)
        {
        }

        public override HttpMethod GetHttpMethod() => HttpMethod.GET;

        public UserDetailsResponse ParseResponse(string content)
        {
            return base.ParseResponse<UserDetailsResponse>(content);
        }
    }
}