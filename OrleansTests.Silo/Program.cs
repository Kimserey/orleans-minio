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
                //.AddMemoryGrainStorage("default")
                .AddMinioGrainStorage("Minio", opts =>
                {
                    opts.AccessKey = "";
                    opts.SecretKey = "";
                    opts.Endpoint = "http://localhost:9000";
                    opts.Container = "ek-grain-state";
                })
                .ConfigureApplicationParts(x =>
                {
                    x.AddFrameworkPart(typeof(MinioGrainStorage).Assembly);
                    x.AddApplicationPart(typeof(BankAccount).Assembly).WithReferences();
                })
                .ConfigureLogging(x => x.AddConsole().AddDebug())
                .Build();

            silo.StartAsync().Wait();

            Console.WriteLine("===> Silo running");
            Console.ReadKey();
        }
    }
}
