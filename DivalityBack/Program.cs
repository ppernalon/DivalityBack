using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using DivalityBack.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DivalityBack
{

    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IWebHost webHost = CreateWebHostBuilder(args).Build();

            // Tests();

            using (var scope = webHost.Services.CreateScope())
            {
                var usersService = scope.ServiceProvider.GetRequiredService<UsersService>();

                usersService.StartMatchmaking();
            }
            await webHost.RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}