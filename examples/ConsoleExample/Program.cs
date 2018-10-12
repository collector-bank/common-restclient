namespace ConsoleExample
{
    using Collector.Common.RestClient;

    using System;
    using System.Linq;

    using ContractsExamples.github.UserDetails;
    using ContractsExamples.github.Users;

    using Microsoft.Extensions.Configuration;

    class Program
    {
        private static readonly string PerformedBy = typeof(Program).Namespace;
        private static IRestApiClient _client;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello ConsoleExample!");
            
            SetupRestClient();

            Console.WriteLine("Make a GET against the github /users end point");

            var users = GetGithubUsers();
            foreach (var user in users)
            {
                Console.WriteLine("Login: " + user.Login);
            }

            Console.WriteLine("Get details for a random user...");
            var randomUser = users.OrderBy(x => Guid.NewGuid().ToString()).First();

            var details = GetUserDetailsResponse(randomUser.Login);

            Console.WriteLine("Details name: " + details.Name);
            Console.WriteLine("Details login: " + details.Login);

            Console.ReadKey();
        }

        private static UsersResponse[] GetGithubUsers()
        {
            var request = new GetUsersRequest(PerformedBy);
            var users = _client.CallAsync(request).Result.ToArray();
            return users;
        }

        private static void SetupRestClient()
        {
            Console.WriteLine("Setup RestClient from appsettings.json");

            var section = new ConfigurationBuilder()
                          .AddJsonFile("appsettings.json")
                          .Build()
                          .GetSection("RestClient");

            _client = new ApiClientBuilder()
                      .ConfigureFromConfigSection(section)
                      .Build();
        }

        private static UserDetailsResponse GetUserDetailsResponse(string login)
        {
            var detailsRequest = new GetUserDetailsRequest(login, PerformedBy);
            var details = _client.CallAsync(detailsRequest).Result;
            return details;
        }
    }
}