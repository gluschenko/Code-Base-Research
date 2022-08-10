﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Wishmaster.Backend.Controllers;

namespace Wishmaster
{
    public class WebStartup
    {
        private readonly IHostEnvironment _env;

        public WebStartup(IHostEnvironment env)
        {
            _env = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllers()
                .AddApplicationPart(Assembly.Load(typeof(AppDataController).Assembly.FullName!))
                .AddNewtonsoftJson(x => x.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseStatusCodePages("text/plain", "Error. Status code: {0}");

            app.Map("/ping", builder => builder.Run(async context =>
            {
                await context.Response.WriteAsync($"OK (uptime: {DateTime.Now - Process.GetCurrentProcess().StartTime})");
            }));

            app.UseRouting();

            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

            if (_env.IsProduction())
            {
                var staticPath = Path.Combine(_env.ContentRootPath, @"client-app\build");
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(staticPath),
                });
            }
            else
            {
                app.UseDeveloperExceptionPage();
                app.UseSpa(spa =>
                {
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:5081");
                });
            }
        }
    }
}
