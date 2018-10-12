namespace ContractsExamples.github.Users
{
    using System.Collections.Generic;

    using Collector.Common.RestContracts;
    using Collector.Common.RestContracts.Interfaces;

    public class GetUsersRequest : GithubApiBaseRequest<UsersResourceIdentifier, List<UsersResponse>>,
        ISuccessfulResponseParser<List<UsersResponse>>
    {
        public GetUsersRequest(string performedBy)
            : base(new UsersResourceIdentifier(), performedBy)
        {
        }

        public override HttpMethod GetHttpMethod() => HttpMethod.GET;
        
        public List<UsersResponse> ParseResponse(string content)
        {
            return base.ParseResponse<List<UsersResponse>>(content);
        }
    }
}