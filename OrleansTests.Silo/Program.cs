using Microsoft.Extensions.Configuration;
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
            var config = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            var silo = new SiloHostBuilder()
                .UseLocalhostClustering()
                .AddMinioGrainStorage("Minio", opts =>
                {
                    opts.AccessKey = config["MINIO_ACCESS_KEY"];
                    opts.SecretKey = config["MINIO_SECRET_KEY"];
                    opts.Endpoint = "localhost:9000";
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
