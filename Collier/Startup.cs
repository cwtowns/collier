using System;
using System.Data.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Collier.IO;
using Collier.Mining;
using Collier.Monitoring;
using Collier.Monitoring.Gpu;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using Collier.Hubs;
using Serilog;
using GpuMonitoringBackgroundService = Collier.Monitoring.Gpu.GpuMonitoringBackgroundService;

namespace GrpcGreeter
{
    public class Startup
    {
        private IServiceCollection _services;
        private IConfiguration _configuration;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        // This method gets called by the runtime. Use this method to add _services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            _services = services;

            _configuration = Program.Config;

            //https://developer.okta.com/blog/2019/11/21/csharp-websockets-tutorial
            _services.AddSignalR();

            _services.AddCollier(_configuration, _cancellationTokenSource);

            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(_configuration).CreateLogger();
        }

        private void OnShutdown()
        {
            _cancellationTokenSource.Cancel();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Microsoft.Extensions.Hosting.IHostApplicationLifetime lifetime)
        {
            lifetime.ApplicationStopping.Register(OnShutdown);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<CollierHub>("/miner");
            });
        }
    }
}

