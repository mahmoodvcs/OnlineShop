using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MahtaKala.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MahtaKala.Helpers;
using MahtaKala.GeneralServices;

namespace MahtaKala
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
            services.AddControllers();
            services.AddApiVersioning();


            //services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.AddSingleton(Configuration);

            services.AddDbContext<DataContext>(options=>
            {
                //options.UseNpgsql(Configuration.GetConnectionString("DataContext")).UseSnakeCaseNamingConvention();
                options.UseSqlServer(Configuration.GetConnectionString("DataContext"));
            });

            services.AddScoped<ISMSService, ParsGreenSMSService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
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
