using EskadDataBringer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EskadDataBringer
{
	public class DataContextSql : EskadContext
	{
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer("Server=Mahta.eskad.ir;Database=Mahta;uid=sa;pwd=@Mahta@&1399!");
		}
	}
}
