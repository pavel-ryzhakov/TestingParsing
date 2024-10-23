using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace ConsoleParser
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureServices((ctx, services) =>
                {
                    services.AddDbContextFactory<PassportDbContext>(options =>
                        options.UseNpgsql("server=localhost;username=postgres;database=passports_db;password=131313"));
                    services.AddScoped<PassportParser>();
                });

            
            var app = builder.Build();

            var parser = app.Services.GetRequiredService<PassportParser>();

            Console.WriteLine("Таймер запущен");
            await parser.ProcessPassportsAsync(@"C:\Users\User\Desktop\Data.csv");

            Console.WriteLine("Все");

            //10000 - 3.00
        }
    }
}
