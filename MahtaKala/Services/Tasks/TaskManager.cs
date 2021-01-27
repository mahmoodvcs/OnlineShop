using MahtaKala.GeneralServices.SMS;
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
            //services.AddSingleton<SMSManager>();
            //services.AddScoped<DeliveryCodeReceiver>();
            
            services.AddScoped<ISMSProcessor, DeliveryCodeReceiver>();

            services.AddScoped<SMSManager>();
            services.AddScoped<ReadReceivedSMSsTask>();
            services.AddScoped<OrphanOrderCatcherService>();
            services.AddScoped<CancelOrphanOrdersTask>();
            //services.AddScoped<CancelOrphanOrdersTask2>();

			services.AddSingleton(new JobSchedule(
				jobType: typeof(ReadReceivedSMSsTask),
        		//cronExpression: "0 0/1 * ? * * *")); // run every minute
        		cronExpression: "0 0/1 18-21 * * ?")); // run every minute, between 6:00pm and 9:59pm
			services.AddSingleton(new JobSchedule(
                jobType: typeof(CancelOrphanOrdersTask),
                cronExpression: "0 0/1 * * * ? *")); // run every odd minute of every hour (except 
                                                     //cronExpression: "0 0 */12 ? * *")); // run every 12 hours
            //services.AddSingleton(new JobSchedule(
            //    jobType: typeof(CancelOrphanOrdersTask2),
            //    cronExpression: "0 0/1 * * * ? *"
            //    ));
        }
    }
}
