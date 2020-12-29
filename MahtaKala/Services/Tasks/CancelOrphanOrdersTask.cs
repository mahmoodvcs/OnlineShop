using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Services.Tasks
{
	//[DisallowConcurrentExecution]
	public class CancelOrphanOrdersTask : IJob
	{
		public CancelOrphanOrdersTask(IServiceProvider provider)
		{
			this.provider = provider;
		}

		public IServiceProvider provider;

		public async Task Execute(IJobExecutionContext context)
		{
			using (var scope = provider.CreateScope())
			{
				var orphanCatcherService = scope.ServiceProvider.GetService<OrphanOrderCatcherService>();
				await orphanCatcherService.RoundUpOrphans();
			}
		}
	}

	public class CancelOrphanOrdersTask2 : IJob
	{
		public CancelOrphanOrdersTask2(IServiceProvider provider)
		{
			this.provider = provider;
		}

		public IServiceProvider provider;

		public async Task Execute(IJobExecutionContext context)
		{
			using (var scope = provider.CreateScope())
			{
				var orphanCatcherService = scope.ServiceProvider.GetService<OrphanOrderCatcherService>();
				await orphanCatcherService.RoundUpOrphans();
			}
		}
	}
}
