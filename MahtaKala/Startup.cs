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
using MahtaKala.Services;
using MahtaKala.Middlewares;
using Microsoft.OpenApi.Models;
using System.IO;
using Swashbuckle.AspNetCore.Filters;
using MahtaKala.ActionFilter;

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
            services.AddApiVersioning(op=>
            {
                op.AssumeDefaultVersionWhenUnspecified = true;
                op.DefaultApiVersion = new ApiVersion(1, 0);
            });

            services.AddVersionedApiExplorer(o =>
            {
                o.GroupNameFormat = "'v'VVV";
                o.SubstituteApiVersionInUrl = true;
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MahtaShop API", Version = "v1" });
                var filePath = Path.Combine(System.AppContext.BaseDirectory, "MahtaKala.xml");
                c.IncludeXmlComments(filePath);

                c.OperationFilter<SwaggerAuthOperationFilter>();

                c.AddSecurityDefinition("JWT", new OpenApiSecurityScheme
                {
                    BearerFormat = "JWT",
                    Scheme = "bearer",
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http
                });
            });

            services.Configure<IISServerOptions>(options =>
            {
                options.AutomaticAuthentication = false;
            });

            //services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.AddSingleton(Configuration);

            services.AddDbContext<DataContext>(options=>
            {
                //options.UseNpgsql(Configuration.GetConnectionString("DataContext")).UseSnakeCaseNamingConvention();
                options.UseSqlServer(Configuration.GetConnectionString("DataContext"));
            });

            services.AddScoped<ISMSService, ParsGreenSMSService>();
            services.AddScoped<IUserService, UserService>();
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
            app.UseMiddleware<JwtMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "MahtaShop API V1");
            });
        }
    }
}
