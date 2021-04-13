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
using MahtaKala.Infrustructure.ActionFilter;
using MahtaKala.Infrustructure.Exceptions;
using MahtaKala.Models.StaffModels;
using MahtaKala.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MahtaKala.Controllers.Staff
{
	[Route("~/Staff/BusinessDept/[action]")]
	[Authorize(UserType.Admin)]
	public class BusinessDeptController : SiteControllerBase<BusinessDeptController>
	{
		private readonly EskaadContext eskaadDb;
		private readonly EskaadService eskaadService;

		private readonly string[] EligibleUsers = { "katouzian", "mosalli", "ali.d" };

		public BusinessDeptController(
			DataContext context,
			ILogger<BusinessDeptController> logger,
			EskaadContext eskaadContext,
			EskaadService eskaadService) : base(context, logger)
		{
			this.eskaadDb = eskaadContext;
			this.eskaadService = eskaadService;
		}

		private bool UserHasTheAuthority()
		{
			if (User == null)
				return false;
			if (string.IsNullOrWhiteSpace(User.Username) || !EligibleUsers.Contains(User.Username))
				return false;
			return true;
		}

		//[Authorize(UserType.Admin, UserType.Staff)]
		//public async Task<IActionResult> Index()
		//{
		//	if (!UserHasTheAuthority())
		//		return RedirectToAction("Index", "Staff");
		//	var now = DateTime.Now;
		//	var alreadyDoneToday = await eskaadService.EskaadOrderAlreadyPlacedToday();
		//	if (alreadyDoneToday)
		//	{
		//		return RedirectToAction("Sales");// View("~/Views/Staff/BusinessDept/Sales");
		//	}
		//	return View();
		//}

		//[HttpGet]
		//public IActionResult EskaadSalesView()
		//{
		//	if (!UserHasTheAuthority())
		//		return RedirectToAction("Index", "Staff");
		//	return View("~/Views/Staff/BusinessDept/EskaadSalesView.cshtml");
		//}

		[HttpGet]
		public async Task<IActionResult> GetEskaadSalesDataSource()
		{
			if (!UserHasTheAuthority())
				return null;
			var sales = await eskaadService.GetEskaadSales();
			return Json(sales);
			//return KendoJson(await salesQuery.ToDataSourceResultAsync(request));
		}

		//[HttpGet]
		//public IActionResult EskaadMerchandise()
		//{
		//	if (!UserHasTheAuthority())
		//		return RedirectToAction("Index", "Staff");
		//	return View("~/Views/Staff/BusinessDept/EskaadMerchandise.cshtml");
		//}

		[HttpGet]
		public async Task<IActionResult> EskaadOrdersAlreadyPlacedForToday()
		{
			var alreadyDoneToday = await eskaadService.EskaadOrderAlreadyPlacedToday();
			return Json(new { alreadyDoneToday });
		}

		[HttpPost]
		public async Task<IActionResult> AddNewOrderItem(string merchandiseCode, int quantity)
		{
			if (!UserHasTheAuthority())
				return Json(new { success = false, message = "Access denied!" });
			(var success, var message) = await eskaadService.AddNewOrderItem(merchandiseCode, quantity);
			return Json(new { success , message });
		}


		[HttpGet]
		//[Microsoft.AspNetCore.Authorization.Authorize(policy: "EskaadAuthorizedUsers")]
		public async Task<IActionResult> GetEskaadMerchandiseDataSource()
		{
			if (!UserHasTheAuthority())
				return null;
			var eskaadMerchandise = eskaadService.GetEskaadMerchandise(false, false);//eskaadDb.Merchandise.Where(x => Convert.ToInt32(x.Place) != 13).AsQueryable();
			return Json(await eskaadMerchandise.ToListAsync());
			//return KendoJson(await eskaadMerchandise.ToDataSourceResultAsync(request));
		}


		[HttpGet]
		public async Task<IActionResult> GetOrderDraftDataSource()
		{
			if (!UserHasTheAuthority())
				return null;
			var orderDraftList = await eskaadService.GetOrderDraftsForToday();
			//return KendoJson(await orderDraftList.ToDataSourceResultAsync(request));
			return Json(orderDraftList);
		}

		[HttpPost]
		//[AjaxAction]
		public async Task<IActionResult> DeleteOrderDraftItem(long id)
		{
			if (!UserHasTheAuthority())
				return Json(new { success = false, message = "Access denied!" });
			(var success, var message) = await eskaadService.DeleteOrderDraftItem(id);
			return Json(new { success, message });
		}

		[HttpPost]
		public async Task<IActionResult> PlaceEskaadOrdersForToday()
		{
			if (!UserHasTheAuthority())
				return Json(new { success = false, message = "Access denied!" });

			//var alreadyOrderedToday = await eskaadService.EskaadOrderAlreadyPlacedToday();
			//if (alreadyOrderedToday)
			//	return Json(new { success = false, message = "سفارش امروز قبلاً ثبت شده است." });
			var now = DateTime.Now;
			var draftsCount = await db.EskaadOrderDrafts.CountAsync(x => x.CreatedDate.Date.Equals(now.Date) && !x.OrderIsSealed);
			if (draftsCount == 0)
			{
				return Json(new { success = false, message = "پیش سفارش برای امروز ثبت نشده است! لطفاً ابتدا کالاهای مورد نظر خود را در پیش سفارش ثبت کنید." });
			}
			try
			{
				int placedOrdersCount = await eskaadService.PlaceToday_sOrdersForEskaad();
				return Json(new { success = true, message = $"تعداد {placedOrdersCount} سفارش در دیتابیس اسکاد ثبت شد." });
			}
			catch (Exception e)
			{
				if (e is ApiException)
				{
					return Json(new { success = false, message = e.Message });
				}
				else
				{
					Exception digger = e;
					string errorMessage = e.Message;
					while (digger.InnerException != null)
					{
						digger = digger.InnerException;
						errorMessage += Environment.NewLine + "Going one level deepr! Inner exception message: " + Environment.NewLine +
							digger.Message;
					}
					logger.LogError($"EskaadService - PlaceEskaadOrdersForToday - Exception: {errorMessage}");
					return Json(new { success = false, message = "خطایی در انجام عملیات رخ داده است! لطفاً با ادمین سیستم تماس بگیرید!" + errorMessage });
				}
			}
			
		}

		#region ProductMatching

		//private async Task<bool> ProductMatchingAlreadyDoneToday()
		//{
		//	var now = DateTime.Now;
		//	var alreadyDoneToday = await db.eskaadMerchandiseToProductMatchings.AnyAsync(x => x.CreatedDate.Date == now.Date);
		//	return alreadyDoneToday;
		//}



		//[HttpPost]
		//[Authorize(UserType.Admin, UserType.Staff)]
		//public async Task<IActionResult> GetEskaadMerchandiseDataSource([DataSourceRequest] DataSourceRequest request)
		//{
		//	var now = DateTime.Now;
		//	var alreadyDoneToday = await EskaadOrderAlreadyPlacedToday();
		//	if (alreadyDoneToday)
		//	{
		//		var localEskaadMerchandise = await db.EskaadMerchandise.Where(x => x.FetchedDate.Date.Equals(now.Date)).ToListAsync();
		//		var resultMerchandiseModels = localEskaadMerchandise.Select(x => new EskaadMerchandiseModel(x)).ToList();
		//		return KendoJson(resultMerchandiseModels.AsQueryable().ToDataSourceResult(request));
		//	}
		//	else
		//	{// If we're here, it means today's data is not present in our database, so we need to do the whole thing, 
		//	 // which starts with Matching, in another page...
		//		return RedirectToAction("ProductMatchings");
		//	}
		//}


		//public async Task<IActionResult> ProductMatchings()
		//{
		//	var now = DateTime.Now;
		//	var alreadyMatchedForToday = await ProductMatchingAlreadyDoneToday();
		//	if (alreadyMatchedForToday)
		//	{
		//		var matchings = db.eskaadMerchandiseToProductMatchings.Where(x => x.CreatedDate.Date.Equals(now.Date)).ToList();
		//		return matchings;
		//	}
		//	var now = DateTime.Now;
		//	var ourProducts = await db.Products.Include(x => x.Quantities)
		//			.Where(x => x.SellerId == ESKAAD_SELLER_ID).ToListAsync();
		//	var eskaadMerchandise = await eskaadDb.Merchandise.ToListAsync();
		//	var merchandiseModels = new List<EskaadMerchandiseModel>();
		//	Dictionary<string, long> processedProductIds = new Dictionary<string, long>();
		//	Dictionary<string, string> mahtaIdToEskaadCodeMatching = new Dictionary<string, string>();

		//	foreach (var eskaadItem in eskaadMerchandise)
		//	{
		//		var eskaadItemCode = eskaadItem.Code.Trim();
		//		if (eskaadItemCode.Length < 7)
		//		{
		//			logger.LogError($"Unrecognized Eskaad product code! The code ({eskaadItemCode}) doesn't contain enough digits! (7 and 8 are the only acceptable code lengths, legend has it!)", eskaadItem.Id);
		//			//throw new Exception("");    // TODO: appropriate exception handling
		//		}
		//		if (eskaadItemCode.Length > 8)
		//		{
		//			logger.LogError($"Unrecognized Eskaad product code! The code ({eskaadItemCode}) is supposed to contain 7 or 8 digits, but, there are more!", eskaadItem.Id);
		//			//throw new Exception("");    // TODO: appropriate exception handling
		//		}
		//		EskaadMerchandiseModel newItem = new EskaadMerchandiseModel(eskaadItem);
		//		string mahta_code = eskaadItemCode.Substring(eskaadItemCode.Length - 6, 6);
		//		if (ourProducts.Count(x => x.Code.Equals(mahta_code)) > 1)
		//			logger.LogError($"This is not good at all! The \"Code\" property in products should be unique, but that's not the case for this code:{mahta_code}");
		//		var ourRespectiveProduct = ourProducts.Where(x => x.Code.Equals(mahta_code)).FirstOrDefault();
		//		bool matchedByCode = true;
		//		if (ourRespectiveProduct == null)
		//		{
		//			matchedByCode = false;
		//			long foundProductId = FindProductBasedOnTitleSimilarities(ourProducts, eskaadItem);
		//			if (foundProductId > 0)
		//			{
		//				ourRespectiveProduct = ourProducts.First(x => x.Id == foundProductId);
		//			}

		//		}
		//		if (ourRespectiveProduct != null)
		//		{
		//			newItem.SetMahtaValues(ourRespectiveProduct);
		//			if (matchedByCode)
		//				newItem.MatchingMethod = MatchingMethod.CodesAreIdentical;
		//			else
		//				newItem.MatchingMethod = MatchingMethod.NamesAreSimilar;

		//			//processedProductCodes.Add(ourRespectiveProduct.Code);
		//			if (processedProductIds.ContainsKey(ourRespectiveProduct.Code))
		//			{
		//				logger.LogError($"Two different products from Eskaad have been matched with the same product from Mahta -  \"{eskaadItemCode}\"");
		//				// TODO: Exception handling!
		//			}
		//			else
		//			{
		//				processedProductIds.Add(ourRespectiveProduct.Code, ourRespectiveProduct.Id);
		//			}
		//		}
		//		newItem.SetItemProirity();
		//		merchandiseModels.Add(newItem);
		//		// We fetched Eskaad's data from their database, and processed it for viewing purposes...
		//		// Now, to save what we copied from Eskaad in our own db...
		//		var merchandiseHistoryItem = new EskaadMerchandise();
		//		merchandiseHistoryItem.Id_Eskaad = newItem.Id_Eskaad;
		//		merchandiseHistoryItem.ProductId_Mahta = newItem.ProductId_Mahta;
		//		merchandiseHistoryItem.Code_Eskaad = newItem.Code_Eskaad;
		//		merchandiseHistoryItem.Code_Mahta = newItem.Code_Mahta;
		//		merchandiseHistoryItem.Name_Eskaad = newItem.Name_Eskaad;
		//		merchandiseHistoryItem.Name_Mahta = newItem.Name_Mahta;
		//		merchandiseHistoryItem.Unit_Eskaad = newItem.Unit_Eskaad;
		//		merchandiseHistoryItem.Count_Eskaad = newItem.Count_Eskaad;
		//		merchandiseHistoryItem.Quantity_Mahta = newItem.Quantity_Mahta;
		//		merchandiseHistoryItem.YellowWarningThreshold_Mahta = newItem.YellowWarningThreshold_Mahta;
		//		merchandiseHistoryItem.RedWarningThreshold_Mahta = newItem.RedWarningThreshold_Mahta;
		//		merchandiseHistoryItem.Place_Eskaad = newItem.Place_Eskaad;
		//		merchandiseHistoryItem.Price_Eskaad = newItem.Price_Eskaad;
		//		merchandiseHistoryItem.Active_Eskaad = newItem.Active_Eskaad;
		//		merchandiseHistoryItem.Status_Mahta = newItem.Status_Mahta;
		//		merchandiseHistoryItem.IsPublished_Mahta = newItem.IsPublished_Mahta;
		//		merchandiseHistoryItem.PresentInEskaad = newItem.PresentInEskaad;
		//		merchandiseHistoryItem.PresentInMahta = newItem.PresentInMahta;
		//		merchandiseHistoryItem.Validation_Eskaad = newItem.Validation_Eskaad;
		//		merchandiseHistoryItem.Tax_Eskaad = newItem.Tax_Eskaad;
		//		merchandiseHistoryItem.FetchedDate = now;
		//		db.EskaadMerchandise.Add(merchandiseHistoryItem);
		//	}
		//	await db.SaveChangesAsync();
		//	foreach (var product in ourProducts)
		//	{
		//		if (processedProductIds.ContainsKey(product.Code))
		//		{
		//			if (processedProductIds[product.Code] == product.Id)
		//				continue;
		//			else
		//			{
		//				// This can not happen! (it'll probably never happen in its whole life time)
		//				// This means we have two products with different Ids and identical codes!
		//				// TODO: Inform the proper authorities to take action against the devil's wrong-doings! This will not stand!
		//				logger.LogError($"EskaadMerchandise -- Product 1;Code:{product.Code}, Id:{product.Id} - " +
		//					$"Product 2; Code:{product.Code}, Id:{processedProductIds[product.Code]} - THIS IS UNACCEPTABLE!");
		//				continue;
		//			}
		//		}
		//		EskaadMerchandiseModel newItem = new EskaadMerchandiseModel(product);
		//		newItem.SetItemProirity();
		//		merchandiseModels.Add(newItem);
		//	}
		//	return KendoJson(merchandiseModels.AsQueryable().ToDataSourceResult(request));
		//}

		private long FindProductBasedOnTitleSimilarities(List<Product> products, Merchandise eskaadMerchandiseItem)
		{
			var eskaadItemTitle = Util.NormalizeStringForWordWiseLooseComparison(eskaadMerchandiseItem.Name);
			List<Product> normalizedProducts = new List<Product>();
			foreach (var product in products)
			{
				var newP = new Product() { Id = product.Id, Title = Util.NormalizeStringForWordWiseLooseComparison(product.Title) };
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

		#endregion ProductMatching

		#region ImportFromExcel

		//[Authorize(UserType.Admin, UserType.Staff)]
		//public async Task<IActionResult> ImportSaleOrdersFromExcel()
		//{
		//	var now = DateTime.Now;
		//	//var excelFileContent = System.IO.File.ReadAllLines("F:\\Workspace\\Files\\EskaadOrdersToBePlaced-1399-09-23-1.csv");
		//	var excelFileContent = System.IO.File.ReadAllLines("F:\\Workspace\\Eskaad Merchandise\\Eskaad Orders To Be Placed -99.09.24.csv");
		//	List<string> unsuccessfulOrderCodes = new List<string>();
		//	List<string> unsuccessfulOrderReasons = new List<string>();
		//	int placedOrdersCount = 0;
		//	int failedOrderCount = 0;

		//	//foreach (var line in excelFileContent)
		//	int codeIndex = 1;
		//	int quantityIndex = 10;
		//	var headerLine = excelFileContent[0].Split(',');
		//	for (int i = 0; i < headerLine.Length; i++)
		//	{
		//		var headerTitle = headerLine[i].ToLower();
		//		switch (headerTitle)
		//		{
		//			case "code":
		//			case "کد":
		//				break;
		//			case "quantity":
		//			case "تعداد درخواستی":
		//				break;

		//		}
		//	}
		//	for (int i = 1; i<excelFileContent.Length; i++)
		//	{
		//		var line = excelFileContent[i];
		//		var values = line.Split(',');
		//		var eskaadCode = values[codeIndex];
		//		//var mahtaCode = values[2];
		//		//if (string.IsNullOrWhiteSpace(mahtaCode))
		//			//mahtaCode = eskaadCode.Substring(1);
		//		int foundCount = await eskaadDb.Merchandise.Where(x => x.Code.Equals(eskaadCode)).CountAsync();
		//		if (foundCount == 0)
		//		{
		//			unsuccessfulOrderCodes.Add(eskaadCode);
		//			unsuccessfulOrderReasons.Add($"The code {eskaadCode} is not present in Eskaad db!");
		//			failedOrderCount++;
		//		}
		//		else if (foundCount > 1)
		//		{
		//			unsuccessfulOrderCodes.Add(eskaadCode);
		//			unsuccessfulOrderReasons.Add($"The code {eskaadCode} has more than one equal in Eskaad db ({foundCount}, actually!)");
		//			failedOrderCount++;
		//		}
		//		else if (foundCount != 1)
		//		{
		//			unsuccessfulOrderCodes.Add(eskaadCode);
		//			unsuccessfulOrderReasons.Add($"Eskaad code: {eskaadCode} - Huh?! This many found:{foundCount}");
		//			failedOrderCount++;
		//		}
		//		else
		//		{
		//			var merchandise = eskaadDb.Merchandise.Where(x => x.Code.Equals(eskaadCode)).First();
		//			int orderQuantity = int.Parse(values[quantityIndex]);
		//			if (orderQuantity > merchandise.Count)
		//			{
		//				unsuccessfulOrderCodes.Add(merchandise.Code);
		//				unsuccessfulOrderReasons.Add($"Eskaad code: {merchandise.Code} - There aren't enough of the item available in" +
		//					$" Eskaad's inventory! They have {merchandise.Count} in stock, but, we want {orderQuantity}, which is " +
		//					$"too much, apparently! Canceling the order!");
		//				failedOrderCount++;
		//				continue;
		//			}
		//			var saleHistoryItem = new EskaadSales();
		//			var eskaadSaleOrder = new MahtaKala.Entities.EskaadEntities.Sales();
		//			eskaadSaleOrder.Code = merchandise.Code;
		//			saleHistoryItem.Code = merchandise.Code;
		//			eskaadSaleOrder.SaleCount = saleHistoryItem.SaleCount = orderQuantity;
		//			if (int.Parse(merchandise.Place) != int.Parse(values[6]))
		//			{
		//				Console.WriteLine("Places do NOT match!");
		//			}
		//			eskaadSaleOrder.Place = saleHistoryItem.Place = merchandise.Place;
		//			eskaadSaleOrder.Date = Util.GetPersianDate(now, true);
		//			saleHistoryItem.Date = eskaadSaleOrder.Date;
		//			eskaadSaleOrder.EskadBankCode = null;
		//			saleHistoryItem.EskadBankCode = null;
		//			eskaadSaleOrder.Transact = null;
		//			saleHistoryItem.Transact = null;
		//			eskaadSaleOrder.SalePrice = saleHistoryItem.SalePrice = 0;
		//			eskaadSaleOrder.MahtaFactorTotal = saleHistoryItem.MahtaFactorTotal = 0;
		//			eskaadSaleOrder.MahtaCountBefore = saleHistoryItem.MahtaCountBefore = 0;
		//			eskaadSaleOrder.MahtaFactor = saleHistoryItem.MahtaFactor = "1000003";
		//			eskaadSaleOrder.Validation = saleHistoryItem.Validation = merchandise.Validation;
		//			eskaadSaleOrder.Flag = saleHistoryItem.Flag = 1;
		//			eskaadDb.Sales.Add(eskaadSaleOrder);
		//			db.EskaadSales.Add(saleHistoryItem);
		//			placedOrdersCount++;
		//		}
		//	}
		//	await eskaadDb.SaveChangesAsync();
		//	await db.SaveChangesAsync();
		//	return Ok(new { SuccessfullyPlaced = placedOrdersCount, Failures = failedOrderCount });
		//}
		//[Authorize(UserType.Admin, UserType.Staff)]
		//public async Task<IActionResult> ImportSaleOrdersFromExcel()
		//{
		//	var now = DateTime.Now;
		//	//var excelFileContent = System.IO.File.ReadAllLines("F:\\Workspace\\Files\\EskaadOrdersToBePlaced-1399-09-23-1.csv");
		//	var excelFileContent = System.IO.File.ReadAllLines("F:\\Workspace\\Files\\EskaadOrdersToBePlaced-1399-09-23-2.csv");
		//	List<string> unsuccessfulOrderCodes = new List<string>();
		//	List<string> unsuccessfulOrderReasons = new List<string>();
		//	int placedOrdersCount = 0;
		//	int failedOrderCount = 0;
		//	if (eskaadDb.Sales.Any())
		//	{
		//		eskaadDb.Sales.RemoveRange(eskaadDb.Sales);
		//		//await eskaadDb.SaveChangesAsync();
		//	}
		//	//foreach (var line in excelFileContent)
		//	for (int i = 1; i<excelFileContent.Length; i++)
		//	{
		//		var line = excelFileContent[i];
		//		var values = line.Split(',');
		//		var eskaadCode = values[1];
		//		var mahtaCode = values[2];
		//		if (string.IsNullOrWhiteSpace(mahtaCode))
		//			mahtaCode = eskaadCode.Substring(1);
		//		int foundCount = await eskaadDb.Merchandise.Where(x => x.Code.Equals(eskaadCode)).CountAsync();
		//		if (foundCount == 0)
		//		{
		//			unsuccessfulOrderCodes.Add(eskaadCode);
		//			unsuccessfulOrderReasons.Add($"The code {eskaadCode} (mahta code: {mahtaCode}) is not present in Eskaad db!");
		//			failedOrderCount++;
		//		}
		//		else if (foundCount > 1)
		//		{
		//			unsuccessfulOrderCodes.Add(eskaadCode);
		//			unsuccessfulOrderReasons.Add($"The code {eskaadCode} (mahta code: {mahtaCode}) has more than one equal in Eskaad db ({foundCount}, actually!)");
		//			failedOrderCount++;
		//		}
		//		else if (foundCount != 1)
		//		{
		//			unsuccessfulOrderCodes.Add(eskaadCode);
		//			unsuccessfulOrderReasons.Add($"Eskaad code: {eskaadCode} - mahta code: {mahtaCode} - Huh?! This many found:{foundCount}");
		//			failedOrderCount++;
		//		}
		//		else
		//		{
		//			var merchandise = eskaadDb.Merchandise.Where(x => x.Code.Equals(eskaadCode)).First();
		//			int orderQuantity = int.Parse(values[11]);
		//			var saleHistoryItem = new EskaadSales();
		//			var eskaadSaleOrder = new MahtaKala.Entities.EskaadEntities.Sales();
		//			eskaadSaleOrder.Code = merchandise.Code;
		//			saleHistoryItem.Code = merchandise.Code;
		//			eskaadSaleOrder.SaleCount = saleHistoryItem.SaleCount = orderQuantity;
		//			if (int.Parse(merchandise.Place) != int.Parse(values[6]))
		//			{
		//				Console.WriteLine("Places do NOT match!");
		//			}
		//			eskaadSaleOrder.Place = saleHistoryItem.Place = merchandise.Place;
		//			eskaadSaleOrder.Date = Util.GetPersianDate(now, true);
		//			saleHistoryItem.Date = eskaadSaleOrder.Date;
		//			eskaadSaleOrder.EskadBankCode = null;
		//			saleHistoryItem.EskadBankCode = null;
		//			eskaadSaleOrder.Transact = null;
		//			saleHistoryItem.Transact = null;
		//			eskaadSaleOrder.SalePrice = saleHistoryItem.SalePrice = 0;
		//			eskaadSaleOrder.MahtaFactorTotal = saleHistoryItem.MahtaFactorTotal = 0;
		//			eskaadSaleOrder.MahtaCountBefore = saleHistoryItem.MahtaCountBefore = 0;
		//			eskaadSaleOrder.MahtaFactor = saleHistoryItem.MahtaFactor = "1000003";
		//			eskaadSaleOrder.Validation = saleHistoryItem.Validation = merchandise.Validation;
		//			eskaadSaleOrder.Flag = saleHistoryItem.Flag = 1;
		//			eskaadDb.Sales.Add(eskaadSaleOrder);
		//			db.EskaadSales.Add(saleHistoryItem);
		//			placedOrdersCount++;
		//		}
		//	}
		//	await eskaadDb.SaveChangesAsync();
		//	await db.SaveChangesAsync();
		//	return Ok(new { SuccessfullyPlaced = placedOrdersCount, Failures = failedOrderCount });
		//}
		#endregion ImportFromExcel
	}
}
