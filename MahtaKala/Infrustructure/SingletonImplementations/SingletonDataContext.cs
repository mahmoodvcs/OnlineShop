using MahtaKala.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure
{
	// This is for technical purposes; we need a DataContext service which could be used by singleton services,
	// and, since our ordinarily-used DataContext service is scoped, it can't be used by a singleton service,
	// so, wee need another DataContext service that is registered as a singleton service, and therefore, is
	// capable of serving singleton services.
	public class SingletonDataContext : DataContext
	{
		public SingletonDataContext()
		{
		}
		public SingletonDataContext(DbContextOptions<SingletonDataContext> options) : base(options)
		{
		}
	}
}
