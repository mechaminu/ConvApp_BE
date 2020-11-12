using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Core.Extensions;
using ConvAppServer.Models;
using ConvAppServer.Controllers;

namespace ConvAppServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILogger<PostingsController> logger)
        {
            Configuration = configuration;
            Logger = logger; 
        }

        public IConfiguration Configuration { get; }
        public ILogger Logger { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // 데이터베이스 Azure SQL로 마이그레이션 완료
            services.AddDbContext<SqlContext>(o =>
                o.UseSqlServer(Configuration.GetConnectionString("SqlDBConnectionString"))
                 .LogTo(new Action<string>(s => Logger.LogInformation(s)), LogLevel.Information));
            services.AddAzureClients(o => 
                o.AddBlobServiceClient(Configuration.GetConnectionString("BlobStorageConnectionString")));

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
    internal static class StartupExtensions
    {
        public static IAzureClientBuilder<BlobServiceClient, BlobClientOptions> AddBlobServiceClient(this AzureClientFactoryBuilder builder, string serviceUriOrConnectionString, bool preferMsi)
        {
            if (preferMsi && Uri.TryCreate(serviceUriOrConnectionString, UriKind.Absolute, out Uri serviceUri))
            {
                return builder.AddBlobServiceClient(serviceUri);
            }
            else
            {
                return builder.AddBlobServiceClient(serviceUriOrConnectionString);
            }
        }
    }
}
