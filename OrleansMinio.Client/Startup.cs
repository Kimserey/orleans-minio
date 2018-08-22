using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using OrleansMinio.GrainInterfaces;
using Swashbuckle.AspNetCore.Swagger;

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

            services.AddSwaggerGen(c => c.SwaggerDoc("v1", new Info { Title = "My Api", Version = "v1" }));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger().UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My Api v1"));

            app.UseMvcWithDefaultRoute();
        }
    }
}
