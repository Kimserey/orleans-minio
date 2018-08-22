using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using OrleansMinio.GrainInterfaces;

namespace OrleansMinio.Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

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
