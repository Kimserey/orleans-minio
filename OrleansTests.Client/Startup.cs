using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using OrleansTests.GrainInterfaces;

namespace OrleansTests.Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var client =
                new ClientBuilder()
                    .UseLocalhostClustering()
                    .ConfigureApplicationParts(x => x.AddApplicationPart(typeof(IBankAccount).Assembly).WithReferences())
                    .ConfigureLogging(logging => logging.AddConsole())
                    .Build();

            client
                .Connect()
                .Wait();

            services.AddSingleton<IGrainFactory>(client);
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
