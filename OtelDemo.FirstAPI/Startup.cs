using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Grpc.Net.Client;
using OtelDemo.Commons;

namespace OtelDemo.FirstAPI
{
    public class Startup
    {
        private ILogger<Startup> _logger;
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var appSettings = Configuration.GetSection("AppSettings").Get<AppSettings>();

            services.AddSingleton(appSettings);
            
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "OtelDemo.FirstAPI", Version = "v1" });
            });
            services.AddTelemetry(appSettings);
        }

        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime applicationLifetime, ILogger<Startup> logger)
        {
            
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            _logger = logger;
            
            applicationLifetime.ApplicationStarted.Register(OnStartUp);
            applicationLifetime.ApplicationStarted.Register(OnShutdown);
            applicationLifetime.ApplicationStopped.Register(OnStop);
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OtelDemo.FirstAPI v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        private void OnStartUp()
        {
            _logger.LogInformation("Aplicação inicializada.");
        }
        
        private void OnShutdown()
        {
            _logger.LogInformation("Aplicação encerrada.");
        }
        
        private void OnStop()
        {
            _logger.LogInformation("Aplicação encerrada.");
        }
    }
}