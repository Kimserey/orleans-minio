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
                .AddJsonFile("appsettings.json")
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
                .ConfigureLogging(x => x
                    .AddConfiguration(config.GetSection("Logging"))
                    .AddConsole()
                    .AddDebug()
                )
                .Build();

            silo.StartAsync().Wait();

            Console.WriteLine("===> Silo running");
            Console.ReadKey();
        }
    }
}
