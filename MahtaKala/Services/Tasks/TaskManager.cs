using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Services.Tasks
{
    public static class TaskManager
    {
        public static void RegisterTasks(IServiceCollection services)
        {
            services.AddHostedService<QuartzHostedService>();

            services.AddSingleton<IJobFactory, SingletonJobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

            services.AddSingleton<ReadReceivedSMSsTask>();
            services.AddSingleton(new JobSchedule(
                jobType: typeof(ReadReceivedSMSsTask),
                cronExpression: "0 0/2 * ? * * *")); // run every 2 minutes
        }
    }
}
