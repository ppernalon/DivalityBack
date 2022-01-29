using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using DivalityBack.Models;
using DivalityBack.Models.Gods;
using DivalityBack.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DivalityBack
{

    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IWebHost webHost = CreateWebHostBuilder(args).Build();
            
            GenericGod anubis_1 = new GenericGod("Anubis_j", 100, 10, 1, 40);
            GenericGod anubis_2 = new GenericGod("Thor_j", 100, 10, 2, 40);
            GenericGod anubis_3 = new GenericGod("Odin_j", 100, 10, 3, 40);
            GenericGod anubis_4 = new GenericGod("Bastet_j", 100, 10, 4, 40);
            GenericGod anubis_5 = new GenericGod("Sauron_j", 100, 10, 6, 40);
            GenericGod anubis_6 = new GenericGod("Poseidon_j", 100, 10, 8, 40);
            GenericGod anubis_7 = new GenericGod("Zeus_p", 100, 10, 1, 80);
            GenericGod anubis_8 = new GenericGod("Samy_p", 100, 10, 4, 80);
            GenericGod anubis_9 = new GenericGod("Tortuga_p", 100, 10, 5, 80);
            GenericGod anubis_10 = new GenericGod("Melkior_p", 100, 10, 3, 80);
            GenericGod anubis_11 = new GenericGod("Adonis_p", 100, 10, 6, 80);
            GenericGod anubis_12 = new GenericGod("Hades_p", 100, 10, 9, 100);

            GodTeam julieTeam = new GodTeam(new[] {anubis_1, anubis_2, anubis_3, anubis_4, anubis_5, anubis_6});
            GodTeam paulTeam = new GodTeam(new[] {anubis_7, anubis_8, anubis_9, anubis_10, anubis_11, anubis_12});
            
            Player julie = new Player(paulTeam, "paul");
            Player paul = new Player(julieTeam, "julie");
            Duel testDuel = new Duel(julie, paul);
            
            testDuel.initDuel();
            while (julie.isAlive() && paul.isAlive())
            {
                testDuel.play();
                Console.WriteLine("turn passed");
            }
            
            Console.WriteLine(testDuel.winner().Username + " a gagné");

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