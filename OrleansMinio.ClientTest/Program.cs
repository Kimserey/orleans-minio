using Microsoft.Extensions.Configuration;
using Minio;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace OrleansMinio.ClientTest
{
    public class BankAccount
    {
        public string Name { get; set; }
        public double Amount { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string line = string.Empty;

            while (line != "exit")
            {
                var data = new BankAccount { Name = "Kim", Amount = new Random().NextDouble() * 100 };

                var config = new ConfigurationBuilder()
                    .AddUserSecrets<Program>()
                    .Build();

                var accessKey = config["MINIO_ACCESS_KEY"];
                var secretKey = config["MINIO_SECRET_KEY"];
                var endpoint = "localhost:9000";
                var container = "test";
                var name = "test.json";

                using (var blob = new MemoryStream())
                {
                    using (var writer = new StreamWriter(blob, Encoding.Default, 1024, true))
                    {
                        writer.Write(JsonConvert.SerializeObject(data));
                    }
                    blob.Position = 0;

                    var sw = Stopwatch.StartNew();
                    var client = new MinioClient(endpoint, accessKey, secretKey);
                    Console.WriteLine("Created client: {0} ms", sw.ElapsedMilliseconds);
                    sw.Restart();

                    // Trimmed of as it is taking 1 seconds.
                    //if (!client.BucketExistsAsync(container).Result)
                    //{
                    //    client.MakeBucketAsync(container).Wait();
                    //}
                    Console.WriteLine("Checked existance of container: {0} ms", sw.ElapsedMilliseconds);
                    sw.Restart();

                    client.PutObjectAsync(container, name, blob, blob.Length).Wait();
                    sw.Stop();
                    Console.WriteLine("Put object: {0} ms", sw.ElapsedMilliseconds);
                }

                line = Console.ReadLine();
            }
        }
    }
}