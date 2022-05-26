using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ProjectorNamer
{
    internal sealed class Program
    {
        private static async Task<int> Main(string[] args)
        {
            var host = CreateHostBuilder(args);
            await host.RunConsoleAsync();
            return Environment.ExitCode;
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<Worker>();
                });
    }
}