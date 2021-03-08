using EskadDataBringer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EskadDataBringer
{
	public class DataContextPg : EskadContext
	{
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseNpgsql("Host=172.16.7.74;Database=mahta;Username=mahta;Password=sry2k3344").UseSnakeCaseNamingConvention();
			//optionsBuilder.UseNpgsql("Host=localhost;Database=mahta2;Username=postgres;Password=1").UseSnakeCaseNamingConvention();
		}
	}
}
