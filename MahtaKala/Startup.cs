using CacheManager.Core;
using EFSecondLevelCache.Core;
using MahtaKala.Entities;
using MahtaKala.Entities.EskaadEntities;
using MahtaKala.Entities.ExceptionHandling;
using MahtaKala.GeneralServices;
using MahtaKala.GeneralServices.Delivery;
using MahtaKala.GeneralServices.Payment;
using MahtaKala.GeneralServices.SMS;
using MahtaKala.Helpers;
using MahtaKala.Infrustructure;
using MahtaKala.Middlewares;
using MahtaKala.Services;
using MahtaKala.Services.Tasks;
using MahtaKala.SharedServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.IO;

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
            services.AddCors();
            //services.AddCors(options => options.AddPolicy(name: "AllowAll",
            //                  builder =>
            //                  {
            //                      builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            //                  }));

            services.AddControllersWithViews();

            services.AddApiVersioning(op =>
            {
                op.AssumeDefaultVersionWhenUnspecified = true;
                op.DefaultApiVersion = new ApiVersion(1, 0);
            });

            services.AddVersionedApiExplorer(o =>
            {
                o.GroupNameFormat = "'v'VVV";
                o.SubstituteApiVersionInUrl = true;
            });

            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
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
            // In production, the Angular files will be served from this directory
            //services.AddSpaStaticFiles(configuration =>
            //{
            //    configuration.RootPath = "ClientApp/dist";
            //});
            services.Configure<IISServerOptions>(options =>
            {
                options.AutomaticAuthentication = false;
            });


            //services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.AddSingleton(Configuration);

            services.AddDbContext<DataContext>(options =>
            {
                options.UseNpgsql(Configuration.GetConnectionString("DataContextPG")).UseSnakeCaseNamingConvention();
            });
            services.AddDbContext<EskaadContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("EskaadSharedDB")/*, x => x.UseNetTopologySuite()*/);
            });
            services.AddMvc(options => { options.UseCustomStringModelBinder(); });
            RegisterMyServices(services, Configuration);
        }

        private static void RegisterMyServices(IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<IUserService, UserService>();
            //services.AddScoped<IBankPaymentService, PardakhtNovinService>();
            services.AddScoped<IBankPaymentService, DamavandEPaymentService>();
            services.AddScoped<IFileService, FileService>();
			//services.AddScoped<ISMSService, PayamSMSV2>();
			//services.AddSingleton<ISMSService, PayamSMSV2>();
			services.AddTransient<ISMSService, PayamSMSV2>();
			//services.AddScoped<OrderService>();
			//services.AddSingleton<OrderService>();
			services.AddTransient<OrderService>();
			services.AddSingleton<AppSettings>();
            services.AddSingleton<IPathService, PathService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            
            services.AddSingleton<IProductImageService, ProductImageService>();
            services.AddSingleton<ICategoryImageService, CategoryImageService>();
            services.AddSingleton<ImagesPathStrategy>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ProductService>();
            services.AddScoped<CategoryService>();
            services.AddScoped<ImportService>();
            services.AddKendo();
            services.AddScoped<SettingsService>();
            services.AddScoped<IDeliveryService, YarBoxDeliveryService>();
            services.AddControllersWithViews().AddRazorRuntimeCompilation();

            TaskManager.RegisterTasks(services, config);

            services.AddEFSecondLevelCache();
            // Add an in-memory cache service provider
            services.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));
            services.AddSingleton(typeof(ICacheManagerConfiguration),
                new CacheManager.Core.ConfigurationBuilder()
                        .WithJsonSerializer()
                        .WithMicrosoftMemoryCacheHandle(instanceName: "MemoryCache1")
                        .WithExpiration(ExpirationMode.Absolute, TimeSpan.FromMinutes(10))
                        .Build());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler("/error");

            FileExtensionContentTypeProvider contentTypes = new FileExtensionContentTypeProvider();
            contentTypes.Mappings[".apk"] = "application/vnd.android.package-archive";
            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = contentTypes
            });
            //if (!env.IsDevelopment())
            //{
            //    app.UseSpaStaticFiles();
            //}

            //app.UseSimpleCaptcha(Configuration.GetSection("BotDetect"));
            app.UseRouting();
            
            app.UseCorsMiddleware();

            app.UseResponseCaching();
            app.UseAuthorization();
            app.UseMiddleware<JwtMiddleware>();
            app.UseSession();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "MahtaShop API V1");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            //app.UseSpa(spa =>
            //{
            //    // To learn more about options for serving an Angular SPA from ASP.NET Core,
            //    // see https://go.microsoft.com/fwlink/?linkid=864501
            //    spa.Options.SourcePath = "ClientApp";
            //    if (env.IsDevelopment())
            //    {
            //        spa.UseAngularCliServer(npmScript: "start");
            //    }
            //});

            MyAppContext.SetHttpContextAccessor(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());
            MyServiceProvider.SetSetviceProvider(app.ApplicationServices.GetRequiredService<IServiceProvider>());
            //SMSManager.SetHttpContextAccessor(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());
            //SMSManager.RegisterProcessorType(typeof(DeliveryCodeReceiver));

            UpdateDatabase(app);

            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var userService = serviceScope.ServiceProvider.GetService<IUserService>();
                userService.CreateAdminUserIfNotExist();
                //var smsManager = serviceScope.ServiceProvider.GetService<SMSManager>();
                //smsManager.RegisterProcessorType(typeof(DeliveryCodeReceiver));
                //var deliveryCodeReceiver = serviceScope.ServiceProvider.GetService<DeliveryCodeReceiver>();
                // Why would someone ever need this kind of complication?! Isn't it a little too much?! Do we really need all these layers of "abstraction" (or, "complication", if we want to be honest!) stacked on top of each other?!

                // Temporarily removed...
            }

            //TODO: consider the performance overhead
            //For Paymernt/Paid?source=api
            //app.Use((context, next) =>
            //{
            //    context.Request.EnableBuffering();
            //    return next();
            //});
        }

        private static void UpdateDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<DataContext>())
                {
                    context.Database.Migrate();
                    SqlErrorParsers.SetDataContext(context);
                }
            }
        }
    }
}
