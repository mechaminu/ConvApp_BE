
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConvAppServer.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var dbAddr = Dns.GetHostAddresses("minuuoo.ddns.net")[0];
            Console.WriteLine(dbAddr);

            services.AddDbContext<ProductContext>(o =>
                o.UseSqlServer($"server={dbAddr},52022;database=products;uid=admin;pwd=admin123"));
            
            services.AddDbContext<PostingContext>(o =>
                o.UseSqlServer($"server={dbAddr},52022;database=postings;uid=admin;pwd=admin123"));
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
