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
            // In order to use multiple processors for received messages, we just need to add the registeration code for
            //    each additional processor service just like the line above. Note that we're ADDing new processors, so,
            //    we shouldn't REMOVE the previously registered processors! So, just add some registeration code lines
            //    to the registerations you already have, like so:
            // services.AddScoped<ISMSProcessor, NewDeliveryCodeReceiverService>();
            // services.AddScoped<ISMSProcessor, AnotherDeliveryCodeReceiverServiceWhichIsDifferent>();
            // AND THAT'S ALL YOU HAVE TO DO TO ADD NEW smsRECEIVERS...

            services.AddScoped<SMSManager>();
            //services.AddTransient<IProvider<DeliveryCodeReceiver>, Provider<DeliveryCodeReceiver>>();
            services.AddScoped<ReadReceivedSMSsTask>();
            services.AddScoped<OrphanOrderCatcherService>();
            services.AddScoped<CancelOrphanOrdersTask>();

			services.AddSingleton(new JobSchedule(
				jobType: typeof(ReadReceivedSMSsTask),
        		//cronExpression: "0 0/1 * ? * * *")); // run every minute
        		cronExpression: "0 0/1 18-21 * * ?")); // run every minute, between 6:00pm and 9:59pm
			services.AddSingleton(new JobSchedule(
                jobType: typeof(CancelOrphanOrdersTask),
                //cronExpression: "0 1,1/2 * * * ? *")); // run every odd minute of every hour (except 
                cronExpression: "0 0/15 * ? * * *")); // run every 15 minutes
        }
    }
}
