using ConvAppServer.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Net;

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
            // �����ͺ��̽��� �̹ο� ���� PC���� ������
            // ���� �����ͺ��̽� : MS SQL 2019
            var dbAddr = Dns.GetHostAddresses("minuuoo.ddns.net")[0];
            services.AddDbContext<ProductContext>(o =>
                o.UseSqlServer($"server={dbAddr},52022;database=products;uid=admin;pwd=admin123"));
            services.AddDbContext<PostingContext>(o =>
                o.UseSqlServer($"server={dbAddr},52022;database=postings;uid=admin;pwd=admin123"));

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
