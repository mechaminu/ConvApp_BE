using ConvAppServer.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Diagnostics;
using System;
using Microsoft.Extensions.Logging;

namespace ConvAppServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // 데이터베이스 Azure SQL로 마이그레이션 완료
            services.AddDbContext<ProductContext>(o =>
                o.UseSqlServer(Configuration.GetConnectionString("ProductsDBConnectionString")));
            services.AddDbContext<PostingContext>(o =>
                o.UseSqlServer(Configuration.GetConnectionString("PostingsDBConnectionString")));

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
}
