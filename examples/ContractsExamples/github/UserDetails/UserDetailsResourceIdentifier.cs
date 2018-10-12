namespace ContractsExamples.github.UserDetails
{
    using Collector.Common.RestContracts;

    public class UserDetailsResourceIdentifier : ResourceIdentifier
    {
        public UserDetailsResourceIdentifier(string login)
        {
            Login = login;
        }

        public override string Uri => $"users/{Login}";

        public object Login { get; set; }
    }
}