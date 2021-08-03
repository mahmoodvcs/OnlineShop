using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Services.Tasks
{
    public class RollbackUnsuccessfulPaymentsTask : IJob
    {
        public RollbackUnsuccessfulPaymentsTask(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public IServiceProvider provider;

        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = provider.CreateScope())
            {
                var orderService = scope.ServiceProvider.GetService<OrderService>();
                await orderService.RollbackUnsuccessfulPayments();
            }
        }
    }
}
