using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Server.Repositories;
using Server.Services;
using System;
using System.IO;
using System.Reflection;

namespace Server
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public FileConfiguration FileConfig { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            FileConfig = Configuration.GetSection("Files").Get<FileConfiguration>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IJSONUserRepository, JSONUserRepository>();
            services.AddSingleton<IJSONUserEventRepository, JSONUserEventRepository>();
            services.AddHostedService<ReposHostedService>();
            services.AddHostedService<EventNotifyHostedService>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Server", Version = "v1" });
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Server v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
