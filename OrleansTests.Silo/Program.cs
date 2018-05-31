using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using OrleansTests.Grains;
using System;
using System.Net;

namespace OrleansTests.Silo
{
    class Program
    {
        static void Main(string[] args)
        {
            var silo = new SiloHostBuilder()
                .UseLocalhostClustering()
                .AddMemoryGrainStorage("default")
                .ConfigureApplicationParts(x =>
                {
                    x.AddApplicationPart(typeof(BankAccount).Assembly).WithReferences();
                })
                .ConfigureLogging(x => x.AddConsole().AddDebug())
                .Build();

            Console.WriteLine("===> Silo running");
            Console.ReadKey();
        }
    }
}
