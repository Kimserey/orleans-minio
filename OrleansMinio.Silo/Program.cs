using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using OrleansMinio.Hosting;
using OrleansMinio.Grains;
using OrleansMinio.Storage;
using System;
using Microsoft.Extensions.Logging.Console;

namespace OrleansMinio.Silo
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
                .AddFileGrainStorage("File", opts =>
                {
                    opts.RootDirectory = "C:/TestFiles";
                })
                .AddMinioGrainStorage("Minio", opts =>
                {
                    opts.AccessKey = config["MINIO_ACCESS_KEY"];
                    opts.SecretKey = config["MINIO_SECRET_KEY"];
                    opts.Endpoint = "localhost:9000";
                    opts.Container = "example-grain-state";
                })
                .ConfigureApplicationParts(x =>
                {
                    x.AddApplicationPart(typeof(BankAccount).Assembly).WithReferences();
                })
                .ConfigureLogging(x => x
                    .AddFilter("System", LogLevel.Information)
                    .AddFilter<ConsoleLoggerProvider>("OrleansMinio.Storage.MinioStorage", LogLevel.Trace)
                    .AddConsole()
                    .AddDebug()
                )
                .Build();

            silo.StartAsync().Wait();

            Console.WriteLine("===> Silo running");
            string input = "";

            while (input != "exit")
            {
                input = Console.ReadLine();
            }
        }
    }
}
