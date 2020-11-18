using MahtaKala.GeneralServices;
using MahtaKala.GeneralServices.SMS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Services.Tasks
{
    [DisallowConcurrentExecution]
    public class ReadReceivedSMSsTask : IJob
    {
        public ReadReceivedSMSsTask(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public IServiceProvider provider;

        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = provider.CreateScope())
            {
                //var smsService = scope.ServiceProvider.GetService<ISMSService>();
                //await smsService.ReadReceivedSMSs();
                var smsManager = scope.ServiceProvider.GetService<SMSManager>();
                await smsManager.FetchAndProcessReceivedSMSes();
            }
        }
    }
}
