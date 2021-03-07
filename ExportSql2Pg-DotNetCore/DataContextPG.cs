using ExportSql2Pg_DotNetCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExportSql2Pg_DotNetCore
{
    internal class DataContextPG : MahtaContext
    {
		//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseNpgsql("Host=172.16.7.74;Database=mahta;Username=mahta;Password=sry2k3344").UseSnakeCaseNamingConvention();
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseNpgsql("Host=localhost;Database=mahta_restored_dec23;Username=postgres;Password=1").UseSnakeCaseNamingConvention();
	}
}
