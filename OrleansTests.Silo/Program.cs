using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using OrleansTests.Grains;
using System;

namespace OrleansTests.Silo
{
    class Program
    {
        static void Main(string[] args)
        {
            var silo = new SiloHostBuilder()
                .UseLocalhostClustering()
                .UseInMemoryReminderService()
                .ConfigureApplicationParts(x =>
                {
                    x.AddApplicationPart(typeof(BankAccount).Assembly).WithReferences();
                })
                .ConfigureLogging(x => x.AddConsole())
                .Build();

            Console.WriteLine("===> Silo running");
            Console.ReadKey();
        }
    }
}
