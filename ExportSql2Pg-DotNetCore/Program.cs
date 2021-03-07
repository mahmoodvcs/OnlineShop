using ExportSql2Pg_DotNetCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExportSql2Pg_DotNetCore
{
	class Program
	{
		static void Main(string[] args)
		{
            DataContextSQL dataContextSql = new DataContextSQL();
            DataContextPG dataContextPg = new DataContextPG();
            Console.WriteLine("Before: " + dataContextPg.Merchandise.Count());
            Console.WriteLine("Eskad: " + dataContextSql.Merchandise.Count());
            dataContextPg.Merchandise.RemoveRange((IEnumerable<Merchandise>)dataContextPg.Merchandise);
            dataContextPg.Sales.RemoveRange((IEnumerable<Sales>)dataContextPg.Sales);
            dataContextPg.SaveChanges();
            dataContextPg.Merchandise.AddRange((IEnumerable<Merchandise>)dataContextSql.Merchandise);
            dataContextPg.Sales.AddRange((IEnumerable<Sales>)dataContextSql.Sales);
            dataContextPg.SaveChanges();
            Console.WriteLine("After: " + dataContextPg.Merchandise.Count());
        }
    }
}
