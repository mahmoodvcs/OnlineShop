using System;
using System.Linq;

namespace EskadDataBringer
{
	internal class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("The fucking thing has started, but wait...");
			DataContextPg pgContext = new DataContextPg();
			DataContextSql sqlContext = new DataContextSql();
			pgContext.Merchandise.RemoveRange(pgContext.Merchandise);
			pgContext.Sales.RemoveRange(pgContext.Sales);
			pgContext.SaveChanges();
			Console.WriteLine("Now, we're gonna get a query from Eskad's Sql Server.");
			var eligibleMerchandiseQuery = sqlContext.Merchandise.Where(x => x.Active == 1 && !(!string.IsNullOrWhiteSpace(x.Place) && x.Place.Contains("13"))).AsQueryable();
			Console.WriteLine($"Eligible merchandise count to be brought over: {eligibleMerchandiseQuery.Count()}");
			Console.WriteLine($"Number of all records available in merchandise table in Eskad's database server: {sqlContext.Merchandise.Count()}");
			pgContext.Merchandise.AddRange(eligibleMerchandiseQuery.ToList());
			pgContext.Sales.AddRange(sqlContext.Sales.ToList());
			pgContext.SaveChanges();
			Console.WriteLine("All done! :)");
		}
	}
}
