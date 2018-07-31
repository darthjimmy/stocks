using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace stocks
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"node_modules")),
                RequestPath = new PathString("/node_modules")
            });
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "try_login", 
                    template: "/login",
                    defaults: new { controller = "Home", action = "Login" }
                );
                routes.MapRoute(
                    name: "try_addStock",
                    template: "/addstock",
                    defaults: new { controller = "Home", action = "AddStock" }
                );
                routes.MapRoute(
                    name: "try_getPrice",
                    template: "/getPrice",
                    defaults: new { controller = "Home", action = "GetPrice" }
                );
                routes.MapRoute(
                    name: "try_getActive",
                    template: "/getMostActive",
                    defaults: new { controller = "Home", action = "MostActive" }
                );
                routes.MapRoute(
                    name: "try_getBalance", 
                    template: "/getBalance",
                    defaults: new { controller = "Home", action = "GetBalance" }
                );
                routes.MapRoute(
                    name: "try_addBalance", 
                    template: "/addBalance",
                    defaults: new { controller = "Home", action = "AddBalance" }
                );
                routes.MapRoute(
                    name: "try_stocks", 
                    template: "/stocks",
                    defaults: new { controller = "Home", action = "doStocks" }
                );
            });
        }
    }
}
