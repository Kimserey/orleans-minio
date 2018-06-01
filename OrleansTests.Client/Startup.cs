using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using OrleansTests.GrainInterfaces;

namespace OrleansTests.Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.Configure<TestOptions>(opt =>
            {
                opt.ValueOne = "1";
                opt.ValueTwo = "2";
            });

            services.PostConfigure<TestOptions>("test", opt =>
            {
                opt.ValueOne = "[POST] " + opt.ValueOne + "|" + opt.ValueTwo;
            });

            services.AddOptions<TestOptions>("test")
                .Configure(test => test.ValueOne = "[test] one")
                .Configure(test => test.ValueOne = "[test] two");

            services.AddSingleton<IGrainFactory>(sp =>
            {
                var client =
                    new ClientBuilder()
                        .UseLocalhostClustering()
                        .ConfigureApplicationParts(x => x.AddApplicationPart(typeof(IBankAccount).Assembly).WithReferences())
                        .ConfigureLogging(logging => logging.AddConsole())
                        .Build();

                client
                    .Connect()
                    .Wait();

                return client;
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvcWithDefaultRoute();
        }
    }
}
