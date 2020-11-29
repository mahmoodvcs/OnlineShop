using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using MahtaKala.ActionFilter;
using MahtaKala.Entities;
using MahtaKala.Entities.EskaadEntities;
using MahtaKala.Helpers;
using MahtaKala.Models.StaffModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MahtaKala.Controllers.Staff
{
	[Route("~/Staff/BusinessDept/[action]")]
	public class BusinessDeptController : SiteControllerBase<BusinessDeptController>
	{
		protected readonly EskaadContext eskaadDb;
		public const long ESKAAD_SELLER_ID = 54;

		public BusinessDeptController(
			DataContext context,
			ILogger<BusinessDeptController> logger,
			EskaadContext eskaadContext) : base(context, logger)
		{
			this.eskaadDb = eskaadContext;
		}

		[Authorize(UserType.Admin, UserType.Staff)]
		public IActionResult Index()
		{
			return View("~/Views/Staff/BusinessDept/Index.cshtml");
		}

		[HttpPost]
		[Authorize(UserType.Admin, UserType.Staff)]
		public async Task<IActionResult> GetEskaadMerchandiseDataSource([DataSourceRequest] DataSourceRequest request)
		{
			var now = DateTime.Now;
			var alreadyDoneToday = await db.EskaadMerchandise.AnyAsync(x => x.FetchedDate.Date.Equals(now.Date));
			if (alreadyDoneToday)
			{
				var localEskaadMerchandise = await db.EskaadMerchandise.Where(x => x.FetchedDate.Date.Equals(now.Date)).ToListAsync();
				var resultMerchandiseModels = localEskaadMerchandise.Select(x => new EskaadMerchandiseModel(x)).ToList();
				return KendoJson(resultMerchandiseModels.AsQueryable().ToDataSourceResult(request));
			}
			else
			{// If we're here, it means today's data is not present in our database, so we need to do the whole thing!
				var ourProducts = await db.Products.Include(x => x.Quantities)
					.Where(x => x.SellerId == ESKAAD_SELLER_ID).ToListAsync();
				var eskaadMerchandise = await eskaadDb.Merchandise.ToListAsync();
				var merchandiseModels = new List<EskaadMerchandiseModel>();
				List<string> processedProductCodes = new List<string>();
				Dictionary<string, long> processedProductIds = new Dictionary<string, long>();
				foreach (var eskaadItem in eskaadMerchandise)
				{
					var eskaadItemCode = eskaadItem.Code.Trim();
					if (eskaadItemCode.Length < 7)
					{
						logger.LogError($"Unrecognized Eskaad product code! The code ({eskaadItemCode}) doesn't contain enough digits! (7 and 8 are the only acceptable code lengths, legend has it!)", eskaadItem.Id);
						//throw new Exception("");    // TODO: appropriate exception handling
					}
					if (eskaadItemCode.Length > 8)
					{
						logger.LogError($"Unrecognized Eskaad product code! The code ({eskaadItemCode}) is supposed to contain 7 or 8 digits, but, there are more!", eskaadItem.Id);
						//throw new Exception("");    // TODO: appropriate exception handling
					}
					EskaadMerchandiseModel newItem = new EskaadMerchandiseModel(eskaadItem);
					string mahta_code = eskaadItemCode.Substring(eskaadItemCode.Length - 6, 6);
					if (ourProducts.Count(x => x.Code.Equals(mahta_code)) > 1)
						logger.LogError($"This is not good at all! The \"Code\" property in products should be unique, but that's not the case for this code:{mahta_code}");
					var ourRespectiveProduct = ourProducts.Where(x => x.Code.Equals(mahta_code)).FirstOrDefault();
					if (ourRespectiveProduct == null)
					{
						long foundProductId = FindProductBasedOnTitleSimilarities(ourProducts, eskaadItem);
						if (foundProductId > 0)
						{
							ourRespectiveProduct = ourProducts.First(x => x.Id == foundProductId);
						}

					}
					if (ourRespectiveProduct != null)
					{
						newItem.SetMahtaValues(ourRespectiveProduct);
						//processedProductCodes.Add(ourRespectiveProduct.Code);
						if (processedProductIds.ContainsKey(ourRespectiveProduct.Code))
						{
							logger.LogError($"There's more than one product in Eskaad's list with their code as \"{eskaadItemCode}\"");
							// TODO: Exception handling!
						}
						else
						{
							processedProductIds.Add(ourRespectiveProduct.Code, ourRespectiveProduct.Id);
						}
					}
					newItem.SetItemProirity();
					merchandiseModels.Add(newItem);
					// We fetched Eskaad's data from their database, and processed it for viewing purposes...
					// Now, to save what we copied from Eskaad in our own db...
					var merchandiseHistoryItem = new EskaadMerchandise();
					merchandiseHistoryItem.Id_Eskaad = newItem.Id_Eskaad;
					merchandiseHistoryItem.ProductId_Mahta = newItem.ProductId_Mahta;
					merchandiseHistoryItem.Code_Eskaad = newItem.Code_Eskaad;
					merchandiseHistoryItem.Code_Mahta = newItem.Code_Mahta;
					merchandiseHistoryItem.Name_Eskaad = newItem.Name_Eskaad;
					merchandiseHistoryItem.Name_Mahta = newItem.Name_Mahta;
					merchandiseHistoryItem.Unit_Eskaad = newItem.Unit_Eskaad;
					merchandiseHistoryItem.Count_Eskaad = newItem.Count_Eskaad;
					merchandiseHistoryItem.Quantity_Mahta = newItem.Quantity_Mahta;
					merchandiseHistoryItem.YellowWarningThreshold_Mahta = newItem.YellowWarningThreshold_Mahta;
					merchandiseHistoryItem.RedWarningThreshold_Mahta = newItem.RedWarningThreshold_Mahta;
					merchandiseHistoryItem.Place_Eskaad = newItem.Place_Eskaad;
					merchandiseHistoryItem.Price_Eskaad = newItem.Price_Eskaad;
					merchandiseHistoryItem.Active_Eskaad = newItem.Active_Eskaad;
					merchandiseHistoryItem.Status_Mahta = newItem.Status_Mahta;
					merchandiseHistoryItem.IsPublished_Mahta = newItem.IsPublished_Mahta;
					merchandiseHistoryItem.PresentInEskaad = newItem.PresentInEskaad;
					merchandiseHistoryItem.PresentInMahta = newItem.PresentInMahta;
					merchandiseHistoryItem.Validation_Eskaad = newItem.Validation_Eskaad;
					merchandiseHistoryItem.Tax_Eskaad = newItem.Tax_Eskaad;
					merchandiseHistoryItem.FetchedDate = now;
					db.EskaadMerchandise.Add(merchandiseHistoryItem);
				}
				await db.SaveChangesAsync();
				foreach (var product in ourProducts)
				{
					if (processedProductIds.ContainsKey(product.Code))
					{
						if (processedProductIds[product.Code] == product.Id)
							continue;
						else
						{
							// This can not happen! (it'll probably never happen in its whole life time)
							// This means we have two products with different Ids and identical codes!
							// TODO: Inform the proper authorities to take action against the devil's wrong-doings! This will not stand!
							logger.LogError($"EskaadMerchandise -- Product 1;Code:{product.Code}, Id:{product.Id} - " +
								$"Product 2; Code:{product.Code}, Id:{processedProductIds[product.Code]} - THIS IS UNACCEPTABLE!");
							continue;
						}
					}
					EskaadMerchandiseModel newItem = new EskaadMerchandiseModel(product);
					newItem.SetItemProirity();
					merchandiseModels.Add(newItem);
				}
				return KendoJson(merchandiseModels.AsQueryable().ToDataSourceResult(request));
			}
		}

		private long FindProductBasedOnTitleSimilarities(List<Product> products, Merchandise eskaadMerchandiseItem)
		{
			var eskaadItemTitle = Util.RemoveExcessWhiteSpaces2(eskaadMerchandiseItem.Name);
			List<Product> normalizedProducts = new List<Product>();
			foreach (var product in products)
			{
				var newP = new Product() { Id = product.Id, Title = Util.RemoveExcessWhiteSpaces2(product.Title) };
				normalizedProducts.Add(newP);
			}
			var ourRespectiveProduct = normalizedProducts.Where(x => x.Title.Equals(eskaadItemTitle)).FirstOrDefault();
			if (ourRespectiveProduct == null)
			{
				int howManyTitlesContainThisOne = normalizedProducts.Count(x => x.Title.Contains(eskaadItemTitle));
				if (howManyTitlesContainThisOne > 0)
				{
					if (howManyTitlesContainThisOne == 1)
					{
						ourRespectiveProduct = normalizedProducts.First(x => x.Title.Contains(eskaadItemTitle));
					}
					else    // This means that there is more than one product with its title containing the title we're currently checking
					{
						//logger.LogInformation()
						// TODO: What to do?! With this kind of bizarre situations?!
					}
				}
			}
			if (ourRespectiveProduct == null)
			{
				int howManyThisTitleContains = normalizedProducts.Count(x => eskaadItemTitle.Contains(x.Title));
				if (howManyThisTitleContains > 0)
				{
					if (howManyThisTitleContains == 1)
					{
						ourRespectiveProduct = normalizedProducts.First(x => eskaadItemTitle.Contains(x.Title));
					}
					else    // This means the set of products - which their titles are substrings of 
							// this title we're currently checking - has more than one member
					{
						// TODO: What to do?! With this kind of bizarre situations?!
					}
				}
			}
			if (ourRespectiveProduct != null)
				return ourRespectiveProduct.Id;
			return 0;
		}

	}
}
