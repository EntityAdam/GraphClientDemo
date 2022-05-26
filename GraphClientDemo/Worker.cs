using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace ProjectorNamer
{
    internal class Worker : IHostedService
    {
        private readonly IHostEnvironment environment;
        private readonly IConfiguration configuration;

        public Worker(IHostEnvironment environment, IConfiguration configuration)
        {
            this.environment = environment;
            this.configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("> Starting Application");
            Console.WriteLine($"Environment: {environment.EnvironmentName}");

            GraphServiceClient graphClient = await GraphClient.SignInAndInitializeGraphServiceClient(configuration.Get<PublicClientApplicationOptions>(), new[] { "user.read" });
            await FetchSignedInUser(graphClient);
            await FetchAllUsers(graphClient);
            await RetrieveAllUserGroups(graphClient);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("> Stopping Application");
            return Task.CompletedTask;
        }

        private static async Task RetrieveAllUserGroups(GraphServiceClient graphClient)
        {
            var groups = await graphClient.Groups.Request().GetAsync();
            foreach(var group in groups)
            {
                Console.WriteLine(group.DisplayName);
            }
        }

        private static async Task FetchSignedInUser(GraphServiceClient graphClient)
        {
            var me = await graphClient.Me.Request().GetAsync();

            // Printing the results
            Console.WriteLine("-------- Fetching Info About Me --------");
            Console.Write(Environment.NewLine);
            Console.WriteLine($"Id: {me.Id}");
            Console.WriteLine($"Display Name: {me.DisplayName}");
            Console.WriteLine($"Email: {me.Mail}");
        }

        private static async Task FetchAllUsers(GraphServiceClient graphClient)
        {
            List<(string, string)> allUsers = new();

            var users = await graphClient.Users
                .Request()
                .Select("DisplayName, Mail")
                .GetAsync();

            allUsers.AddRange(users.Select(x => (x.DisplayName, x.Mail)));

            while (users.NextPageRequest is not null)
            {
                users = await users.NextPageRequest.GetAsync();
                allUsers.AddRange(users.Select(x => (x.DisplayName, x.Mail)));
            }

            Console.WriteLine("--------- Fetch All Users -------");
            foreach (var user in allUsers)
            {
                Console.WriteLine($"DisplayName: {user.Item1} Mail: {user.Item2}");
            }
        }
    }
}