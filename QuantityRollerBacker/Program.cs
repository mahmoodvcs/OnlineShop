using MahtaKala.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Extensions.Logging;
using CacheManager.Core;
using EFSecondLevelCache.Core;
using System.Collections.Generic;

namespace QuantityRollerBacker
{
	class Program
	{
		private static List<string> outputContent;
		private static string TimeTag = "NOTYET";
		private static string OutputDir = "";
		private static void InitiateOutput()
		{
			outputContent = new List<string>();
		}
		private static void OutputToOutputs(string entry)
		{
			if (outputContent == null)
				throw new Exception("OutputContent is not initialized! THIS IS NOT THE TIME TO JOKE AROUND, bitch!");
			Console.WriteLine(entry);
			outputContent.Add(entry);
		}
		private static void FinalizeOutputs()
		{
			if (outputContent == null || outputContent.Count == 0)
				throw new Exception("The output is empty! Not actually appropriate to take notes from nothing!");
			File.WriteAllLines(Path.Combine(OutputDir, $"actualOutput-{TimeTag}"), outputContent);
		}
		private const string _MoreThanOneProductForCode = "MoreThanOneProductForCode";
		private const string _ProductWithCodeDoesNotExist = "ProductWithCodeDoesNotExist";
		private const string _NoQuantityForProduct = "NoQuantityForProduct";
		private const string _MoreThanOneQuantity = "MoreThanOneQuantity";
		static async Task Main(string[] args)
		{
			var startTime = DateTime.Now;
			string connectionString = "Host=localhost;Database=mahta3;Username=postgres;Password=1";
			var provider = "Npgsql.EntityFrameWorkCore.PostgreSql";

			var services = new ServiceCollection().AddLogging(l => l.AddConsole())
				.Configure<LoggerFilterOptions>(c => c.MinLevel = LogLevel.Trace)
				.AddDbContext<DataContext>(options =>
				{
					options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
				});
			services.AddEFSecondLevelCache();
			// Add an in-memory cache service provider
			services.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));
			services.AddSingleton(typeof(ICacheManagerConfiguration),
				new CacheManager.Core.ConfigurationBuilder()
						.WithJsonSerializer()
						.WithMicrosoftMemoryCacheHandle(instanceName: "MemoryCache1")
						.WithExpiration(ExpirationMode.Absolute, TimeSpan.FromMinutes(10))
						.Build());


			var serviceProvider = services.BuildServiceProvider();
			var context = serviceProvider.GetService<DataContext>();
			//outputContent = new List<string>();
			InitiateOutput();
			//var logger = serviceProvider.GetService<ILogger>();
			//var baseAddress = @"E:\My Documents\Programming\Mahta Workspace - Started 1399-07-16\From gitlab.mahtakala.ir\File\Quantity\";
			TimeTag = startTime.ToString("MM-dd HH:mm:ss");
			var basePath = Directory.GetCurrentDirectory();
			OutputDir = Path.Combine(basePath, $"OutputFiles-{TimeTag}");
			if (!Directory.Exists(OutputDir))
				Directory.CreateDirectory(OutputDir);
			if (basePath.Contains("dehghan") && basePath.Contains("home"))
			{
			}
			string inputFilePath = Path.Combine(basePath, "quantitiesToBeSet.csv");
			var fileContent = File.ReadAllLines(inputFilePath)
				.Select(x => x.Split(',')).ToList();

			//var fileContent = File.ReadAllLines(@"E:\My Documents\Programming\Mahta Workspace - Started 1399-07-16\From gitlab.mahtakala.ir\File\Quantity\quantities.csv")
			//.Select(x => x.Split(',')).ToList();
			var mistakenlyRolledBackPaidOrders = context.Orders.Include(x => x.Items).ThenInclude(x => x.ProductPrice)
				.ThenInclude(x => x.Product).ThenInclude(x => x.Quantities)
				.Where(x => x.State == OrderState.Paid && x.CheckOutDate > new DateTime(2020, 11, 28))
				.ToList();
			Dictionary<string, int> ExcessQuantitiesForCodes = new Dictionary<string, int>();
			foreach (var order in mistakenlyRolledBackPaidOrders)
			{
				foreach (var item in order.Items)
				{
					var code = item.ProductPrice.Product.Code;
					if (string.IsNullOrWhiteSpace(code))
						throw new Exception("Code is empty!");
					if (ExcessQuantitiesForCodes.ContainsKey(code))
					{
						ExcessQuantitiesForCodes[code] += item.Quantity;
					}
					else
					{
						ExcessQuantitiesForCodes[code] = item.Quantity;
					}
				}
			}
			File.WriteAllLines(Path.Combine(OutputDir, $"MistakenlyRolledBackOrders-{TimeTag}")
				, ExcessQuantitiesForCodes.Select(x => $"{x.Key} - ShoudBeSubtracted: {x.Value}").ToList());
			List<long> productIdsDealtWith = new List<long>(fileContent.Count);
			List<string> okProductCodes = new List<string>();
			Dictionary<string, string> productCodesNeedingUrgentAttention = new Dictionary<string, string>();
			for (int i = 1; i < fileContent.Count; i++)
			{
				var row = fileContent[i];
				string code = row[0].ToLower();
				bool hasExcessQ = false;
				string hasExcessMessage = "";
				if (ExcessQuantitiesForCodes.ContainsKey(code))
				{
					hasExcessQ = true;
					hasExcessMessage = $"Also, the target quantity is off by {ExcessQuantitiesForCodes[code]}";
				}
				var eligibleCandidates = await context.Products.Include(x => x.Quantities)
					.Where(x => x.Code.ToLower().Equals(code)).ToListAsync();
				if (eligibleCandidates.Count() > 1)
				{
					string outputEntry = $"There are {eligibleCandidates.Count()} products matching the code {code} " +
						$"- ProductIds: {string.Join(',', eligibleCandidates.Select(x => x.Id.ToString()))}";
					if (hasExcessQ)
					{
						outputEntry += Environment.NewLine + hasExcessMessage;
					}
					Console.WriteLine(outputEntry);
					outputContent.Add(outputEntry);
					productCodesNeedingUrgentAttention.Add(code, _MoreThanOneProductForCode + hasExcessMessage);
					continue;
				}
				var product = eligibleCandidates.FirstOrDefault();
				if (product == null)
				{
					string outputEntry = $"Product with code {code} does not exist";
					if (hasExcessQ)
					{
						outputEntry += Environment.NewLine + hasExcessMessage;
					}
					//logger.LogError($"Product with code {code} does not exist");
					OutputToOutputs(outputEntry);
					productCodesNeedingUrgentAttention.Add(code, _ProductWithCodeDoesNotExist + hasExcessMessage);
					continue;
				}

				productIdsDealtWith.Add(product.Id);
				var targetQuantity = int.Parse(row[1]);
				if (product.Quantities.Count() == 0)
				{
					var outputEntry = $"ProductId: {product.Id} - No quantity object for product with code {code}";
					if (hasExcessQ)
					{
						outputEntry += Environment.NewLine + hasExcessMessage;
					}
					OutputToOutputs(outputEntry);
					productCodesNeedingUrgentAttention.Add(code, _NoQuantityForProduct + hasExcessMessage);
					continue;
				}
				if (product.Quantities.Count() > 1)
				{
					var outputEntry = $"ProductId: {product.Id} - It has more than one quantity object ({product.Quantities.Count()}, actually)";
					if (hasExcessQ)
					{
						outputEntry += Environment.NewLine + hasExcessMessage;
					}
					OutputToOutputs(outputEntry);
					productCodesNeedingUrgentAttention.Add(code, _MoreThanOneQuantity + hasExcessMessage);
					continue;
				}
				if (product.Quantities.Single().Quantity == targetQuantity)
				{
					//Console.WriteLine($"Product with code {code} (id:{product.Id}) has the correct quantity: {targetQuantity}");
					//Console.WriteLine("Press any key to move on...");
					//Console.ReadKey();
					okProductCodes.Add(code);
					string outputEntry = $"<<QuantityIsOk>>;Code: {code} - Id: {product.Id} - " +
						$"Quantity: {product.Quantities.Single().Quantity} = {targetQuantity}";
					if (hasExcessQ)
					{
						outputEntry += Environment.NewLine + hasExcessMessage;
					}
					OutputToOutputs(outputEntry);
				}
				else
				{
					string outputEntry = $"Code: {code} - Id: {product.Id} - " +
						$"InventoryQuantity: {product.Quantities.Single().Quantity} - CorrectQuantityToBeSet: {targetQuantity}";
					if (hasExcessQ)
					{
						outputEntry += Environment.NewLine + hasExcessMessage;
					}
					OutputToOutputs(outputEntry);
					//Console.WriteLine("Set correct quantity?");
					//var ans = Console.ReadLine();
					//if (ans.ToLower().StartsWith('y'))
					//{
					//	await SetQuantity(product, targetQuantity, serviceProvider);
					//}
				}
			}
			var remainingProducts = await context.Products.Include(x => x.Quantities)
				.Include(x => x.Price).ThenInclude(x => x.OrderItems).ThenInclude(x => x.Order)
				.Where(x => !productIdsDealtWith.Contains(x.Id)).ToListAsync();
			var remaining_havingOneQuantity = remainingProducts.Where(x => x.Quantities.Count() == 1).ToList();
			var quantity99Count = remaining_havingOneQuantity.Count(x => x.Quantities.First().Quantity != 99);
			var non99Count = remaining_havingOneQuantity.Count() - quantity99Count;
			var entry = $"There remains {remainingProducts.Count()} products, out of which" +
				$" {remaining_havingOneQuantity.Count()} (correctly) have only one quantity! ";
			OutputToOutputs(entry);
			entry = $"of these {remaining_havingOneQuantity.Count()} structurally correct products, " +
				$"{quantity99Count} have a 99 quantity value; and {non99Count} don't!";
			OutputToOutputs(entry);
			List<string> outputForRemaining = new List<string>();
			outputForRemaining.AddRange(remaining_havingOneQuantity
				.Select(x => $"Code: {x.Code} -Id: {x.Id} - Quantity: {x.Quantities.First().Quantity}"));
			outputForRemaining.AddRange(remainingProducts.Except(remaining_havingOneQuantity)
				.Select(x => $"<<QuantityCountNotOne>>;Code: {x.Code} - Id: {x.Id} - QuantityCount: {x.Quantities.Count()}"));
			string remainingOutputFilePath = Path.Combine(OutputDir, $"remaining products-{TimeTag}.out");
			File.WriteAllLines(remainingOutputFilePath, outputForRemaining);

			string urgentOutputFilePath = Path.Combine(OutputDir, $"urgent products-{TimeTag}.out");
			var urgentOutput = new List<string>();
			var list_MoreQuantity = new List<string>();
			var list_NoQuantity = new List<string>();
			var list_MoreThan1Products = new List<string>();
			var list_NoProducts = new List<string>();
			var list_impossible = new List<string>();
			foreach (var key in productCodesNeedingUrgentAttention.Keys)
			{
				switch (productCodesNeedingUrgentAttention[key])
				{
					case _MoreThanOneProductForCode:
						list_MoreThan1Products.Add($"{key} - {_MoreThanOneProductForCode}");
						break;
					case _ProductWithCodeDoesNotExist:
						list_NoProducts.Add($"{key} - {_ProductWithCodeDoesNotExist}");
						break;
					case _MoreThanOneQuantity:
						list_MoreQuantity.Add($"{key} - {_MoreThanOneQuantity}");
						break;
					case _NoQuantityForProduct:
						list_NoQuantity.Add($"{key} - {_NoQuantityForProduct}");
						break;
					default:
						list_impossible.Add($"IMPOSSIBLE: {key} - {productCodesNeedingUrgentAttention[key]}");
						break;
				}
			}
			urgentOutput.AddRange(list_MoreThan1Products);
			urgentOutput.AddRange(list_NoProducts);
			urgentOutput.AddRange(list_MoreQuantity);
			urgentOutput.AddRange(list_NoQuantity);
			urgentOutput.AddRange(list_impossible);

			File.WriteAllLines(urgentOutputFilePath, urgentOutput);

			FinalizeOutputs();
		}

		public static async Task SetQuantity(Product product, int targetQuantity, ServiceProvider serviceProvider)
		{
			var db = serviceProvider.GetService<DataContext>();
			using var transaction = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromSeconds(30), TransactionScopeAsyncFlowOption.Enabled);
			var quantities = await db.ProductQuantities.FromSqlRaw<ProductQuantity>($"SELECT* FROM product_quantities pq WHERE pq.product_id in ({product.Id}) FOR UPDATE").ToListAsync();
			var quantity = quantities.First();
			bool itWasZeroBeforeRollBack = (quantity.Quantity == 0);
			quantity.Quantity = targetQuantity;
			if (product.Status == ProductStatus.NotAvailable && quantity.Quantity > 0 && itWasZeroBeforeRollBack)
			{
				product.Status = ProductStatus.Available;
			}
			await db.SaveChangesAsync();
			transaction.Complete();
		}

	}
}
