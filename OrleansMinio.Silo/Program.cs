using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;
using OrleansMinio.Hosting;
using OrleansMinio.Grains;
using OrleansMinio.Storage;
using System;
using Microsoft.Extensions.Logging.Console;
using Orleans.Configuration;

namespace OrleansMinio.Silo
{
    class Program
    {
        static void Main(string[] args)
        {
            var silo = new SiloHostBuilder()
                .UseLocalhostClustering()
                .AddMinioGrainStorage("Minio", opts =>
                {
                    // Example of settings
                    // minio.exe -C C:\Tools\example server C:\Tools\example
                    opts.AccessKey = "CCZ9CNVPJ1YU5GU4B87J";
                    opts.SecretKey = "jqnrRHkgbAWkf2IrqYIbMo8WJ+NBSVv3WQ1z8oRC";
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
