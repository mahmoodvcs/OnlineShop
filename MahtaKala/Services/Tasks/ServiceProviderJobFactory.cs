using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;

namespace MahtaKala.Services.Tasks
{


	public class ServiceProviderJobFactory : IJobFactory
	{
		private readonly IServiceProvider _rootServiceProvider;
		private ConcurrentDictionary<IJob, IServiceScope> _scopes = new ConcurrentDictionary<IJob, IServiceScope>();

		public ServiceProviderJobFactory(IServiceProvider rootServiceProvider)
		{
			_rootServiceProvider = rootServiceProvider ?? throw new ArgumentNullException("rootServiceProvider");
		}

		public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
		{
			var jobType = bundle.JobDetail.JobType;
			var scope = _rootServiceProvider.CreateScope();
			var job = (IJob)scope.ServiceProvider.GetRequiredService(jobType);

			_scopes.TryAdd(job, scope);
			return job;

			//var jobType = bundle.JobDetail.JobType;

			//// MA - Generate a scope for the job, this allows the job to be registered
			////	using .AddScoped<T>() which means we can use scoped dependencies 
			////	e.g. database contexts
			//var scope = _scopes.GetOrAdd(jobType, t => _rootServiceProvider.CreateScope());

			//return (IJob)scope.ServiceProvider.GetRequiredService(jobType);
		}

		public void ReturnJob(IJob job)
		{
			if (job != null && _scopes.TryGetValue(job, out var scope))
			{
				scope.Dispose();
				_scopes.TryRemove(job, out _);
			}
			//var jobType = job?.GetType();
			//if (job != null && _scopes.TryGetValue(jobType, out var scope))
			//{
			//	//  MA - Dispose of the scope, which disposes of the job's dependencies
			//	scope.Dispose();

			//	// MA - Remove the scope so the next time the job is resolved, 
			//	//	we can get a new job instance
			//	_scopes.TryRemove(jobType, out _);
			//}
		}
	}
}
