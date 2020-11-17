using MahtaKala.Infrustructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Services.Tasks
{
    public static class TaskManager
    {
        public static void RegisterTasks(IServiceCollection services, IConfiguration configuration)
        {
            services.AddHostedService<QuartzHostedService>();

            //services.AddSingleton<IJobFactory, SingletonJobFactory>();
            services.AddSingleton<IJobFactory, ServiceProviderJobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            //services.AddDbContext<SingletonDataContext>(options =>
            //{
            //    options.UseNpgsql(configuration.GetConnectionString("DataContextPG")).UseSnakeCaseNamingConvention();
            //},
            //ServiceLifetime.Singleton);

            services.AddScoped<DeliveryCodeReceiver>();
            services.AddScoped<ReadReceivedSMSsTask>();
            services.AddScoped<OrphanOrderCatcherService>();
            services.AddScoped<CancelOrphanOrdersTask>();

            //services.AddSingleton(new JobSchedule(
            //    jobType: typeof(ReadReceivedSMSsTask),
            //    cronExpression: "0 0/2 * ? * * *")); // run every 2 minutes
            services.AddSingleton(new JobSchedule(
                jobType: typeof(CancelOrphanOrdersTask),
                //cronExpression: "0 1/2 * * * ? *")); // run every odd minute of every hour
                cronExpression: "0 0/1 18-21 ? * * *")); // "0 0/1 6-9 * * ?" : run between 6pm and 10pm, every minute
        }
    }
}
