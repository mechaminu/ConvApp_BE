using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConvAppServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }
        public ILogger Logger { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<MainContext>(o =>
                o.UseSqlServer(Configuration.GetConnectionString("ConvAppBE-DBConnectionString")));
            //o.UseSqlServer("Data Source=localhost;Initial Catalog=convapp;Integrated Security=True"));

            services.AddAzureClients(o =>
                o.AddBlobServiceClient(Configuration.GetConnectionString("BlobStorageConnectionString")));

            services.AddMvc()
                .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);

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
