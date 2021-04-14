using MahtaKala.Entities;
using MahtaKala.Entities.EskaadEntities;
using MahtaKala.Helpers;
using MahtaKala.Infrustructure.Exceptions;
using MahtaKala.Models.StaffModels;
using MahtaKala.SharedServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MahtaKala.Services
{
	public class EskadServiceHttpClient
	{
		private readonly ICurrentUserService currentUserService;
		private readonly ILogger<EskadServiceHttpClient> logger;
		private readonly DataContext dbContext;
		//private readonly EskaadContext eskaadDbContext;
		private readonly HttpClient httpClient;

		public const long ESKAAD_SELLER_ID = 54;
		public const int MAHTA_STORAGE_NUMBER_IN_ESKAAD = 13;
		public static readonly string[] ESKAAD_AUTHORIZED_USERS = { "katouzian", "mosalli", "ali.d" };

		public const string ESKAD_COMMUNICATOR_API_ADDRESS = "host90:8088/api/EskadService/";

		User User => currentUserService.User;

		public EskadServiceHttpClient(ICurrentUserService currentUserService
							, ILogger<EskadServiceHttpClient> logger
							, DataContext dbContext
							//, EskaadContext eskaadContext)
							, HttpClient client)
		{
			this.currentUserService = currentUserService;
			this.logger = logger;
			this.dbContext = dbContext;
			//this.eskaadDbContext = eskaadContext;
			httpClient = client;
			httpClient.BaseAddress = new Uri(ESKAD_COMMUNICATOR_API_ADDRESS);
			httpClient.DefaultRequestHeaders.Accept.Clear();
			httpClient.DefaultRequestHeaders.Accept.Add(
				new MediaTypeWithQualityHeaderValue("application/json"));
		}

		public async Task<IQueryable<Merchandise>> CallGetEskadMerchandiseData(string token)//, bool onlyActive = true, bool removeIfNotInStock = true)
		{
			httpClient.DefaultRequestHeaders.Add("Authorization", "bearer " + token);
			var response = await httpClient.GetAsync("GetEskadMerchandiseData");
			response.EnsureSuccessStatusCode();
			using var responseStream = await response.Content.ReadAsStreamAsync();
			return (await JsonSerializer.DeserializeAsync<IEnumerable<Merchandise>>(responseStream)).AsQueryable();
		}

		//public IQueryable<Merchandise> GetEskaadMerchandise(bool onlyActive = true, bool removeIfNotInStock = true)
		//{
		//	var merchandise = eskaadDbContext.Merchandise.Where(x => Convert.ToInt32(x.Place) != MAHTA_STORAGE_NUMBER_IN_ESKAAD);
		//	if (onlyActive)
		//	{
		//		merchandise = merchandise.Where(x => x.Active == 1);
		//	}
		//	if (removeIfNotInStock)
		//	{
		//		merchandise = merchandise.Where(x => x.Count > 0);
		//	}
		//	return merchandise;
		//}

		public async Task<IQueryable<EskaadOrderDraft>> CallGetEskaadOrderDrafts(string token)
		{
			httpClient.DefaultRequestHeaders.Add("Authorization", "bearer " + token);
			var response = await httpClient.GetAsync("GetEskadOrderDraftData");
			response.EnsureSuccessStatusCode();
			using var responseStream = await response.Content.ReadAsStreamAsync();
			return (await JsonSerializer.DeserializeAsync<IEnumerable<EskaadOrderDraft>>(responseStream)).AsQueryable();
		}

		public async Task<bool> TodaysOrdersAreSealed()
		{
			var now = DateTime.Now;
			var ordersAreSealed = await dbContext.EskaadOrderDrafts.Where(x =>
					x.CreatedDate.Date.Equals(now.Date) && x.OrderIsSealed)
				.AnyAsync();
			return ordersAreSealed;
		}

		//public async Task<bool> EskaadOrderAlreadyPlacedToday()
		//{
		//	var now = DateTime.Now;
		//	var todayPersian = Util.GetPersianDate(now, true);
		//	var todayPersianWithLeadingZeros = Util.GetPersianDate(now, true, true);
		//	var alreadyDoneToday = await eskaadDbContext.Sales
		//		.AnyAsync(x => x.Date.Equals(todayPersian) || x.Date.Equals(todayPersianWithLeadingZeros));
		//	return alreadyDoneToday;
		//}

		public async Task<(bool, string)> CallAddNewOrderItem(string token, string code, int quantity)
		{
			httpClient.DefaultRequestHeaders.Add("Authorization", "bearer " + token);

			var requestBody = new StringContent(
				JsonSerializer.Serialize(new { merchandiseCode = code, quantity }), 
				Encoding.UTF8, 
				"application/json");

			var response = await httpClient.PostAsync("AddNewOrderItem", requestBody);
			response.EnsureSuccessStatusCode();
			using var responseStream = await response.Content.ReadAsStreamAsync();
			return (await JsonSerializer.DeserializeAsync<(bool, string)>(responseStream));
		}

		//public async Task<(bool, string)> AddNewOrderItem(string code, int quantity)
		//{
		//	var now = DateTime.Now;
		//	var todayPersian = Util.GetPersianDate(now, true, true);
		//	string message = "";
		//	//if (await dbContext.EskaadOrderDrafts
		//	//	.Include(x => x.CreatedBy)
		//	//	.Include(x => x.UpdatedBy)
		//	//	.AnyAsync(x =>
		//	//		x.CreatedDate.Date.Equals(now.Date)
		//	//		&& x.OrderIsSealed))
		//	//{
		//	//	//throw new ApiException(512, $"سفارشات امروز ({todayPersian}) بسته شده اند! امکان افزودن سفارش جدید برای امروز وجود ندارد!");
		//	//	return (false, $"سفارشات امروز ({todayPersian}) بسته شده اند! امکان افزودن سفارش جدید برای امروز وجود ندارد!");
		//	//}
		//	var draftItemsQuery = dbContext.EskaadOrderDrafts.Where(x =>
		//			x.CreatedDate.Date.Equals(now.Date) && x.EskaadCode.Equals(code) && !x.OrderIsSealed)
		//		.AsQueryable();
		//	var draftCount = await draftItemsQuery.CountAsync();
		//	if (draftCount > 1)
		//	{
		//		var allDraftItems = await draftItemsQuery.OrderByDescending(x => x.CreatedDate).ToListAsync();
		//		for (int i = 1; i < allDraftItems.Count; i++)
		//			dbContext.EskaadOrderDrafts.Remove(allDraftItems[i]);
		//		await dbContext.SaveChangesAsync();
		//	}
		//	var eskaadOrderDraftItem = await draftItemsQuery.FirstOrDefaultAsync();

		//	if (!await eskaadDbContext.Merchandise.AnyAsync(x => x.Code.Equals(code)
		//			&& Convert.ToInt32(x.Place) != MAHTA_STORAGE_NUMBER_IN_ESKAAD
		//			&& x.Active == 1))
		//	{
		//		message = "";
		//		//if (await dbContext.EskaadOrderDrafts.AnyAsync(x => x.CreatedDate.Equals(now.Date) && x.EskaadCode.Equals(code)))
		//		if (eskaadOrderDraftItem != null)
		//		{
		//			dbContext.EskaadOrderDrafts.Remove(eskaadOrderDraftItem);
		//			await dbContext.SaveChangesAsync();
		//			message = $"آیتم از پیش ثبت شده ی امروز با کد {code} نیز از لیست لیست حذف می شود.";
		//		}
		//		message = $"کالایی با کد {code} در دیتابیس اسکاد وجود ندارد!" + Environment.NewLine + message;
		//		//throw new ApiException(512, errorMessage);
		//		return (false, message);
		//	}
		//	var merchandiseItem = await eskaadDbContext.Merchandise.Where(x => x.Code.Equals(code)
		//			&& Convert.ToInt32(x.Place) != MAHTA_STORAGE_NUMBER_IN_ESKAAD
		//			&& x.Active == 1)
		//		.FirstOrDefaultAsync();
		//	if (merchandiseItem.Count < quantity)
		//	{
		//		message = "";
		//		if (eskaadOrderDraftItem != null)
		//		{
		//			if (merchandiseItem.Count < eskaadOrderDraftItem.Quantity)
		//			{
		//				message = $"آیتم قبلاً ثبت شده ی امروز با کد {code} نیز از لیست حذف می شود - تعداد ثبت شده: {eskaadOrderDraftItem.Quantity}";
		//				dbContext.EskaadOrderDrafts.Remove(eskaadOrderDraftItem);
		//				await dbContext.SaveChangesAsync();
		//			}
		//			else
		//			{
		//				message = $"ولی آیتم قبلاً ثبت شده ی امروز، با تعداد سفارش {eskaadOrderDraftItem.Quantity} در لیست باقی خواهد ماند.";
		//			}
		//		}
		//		message = $"کالای {merchandiseItem.Name} با کد {merchandiseItem.Code} به تعداد کافی در"
		//			+ $" انبار اسکاد موجود نیست! تعداد موجود: {merchandiseItem.Count} - تعداد مورد نیاز: {quantity}"
		//			+ message;
		//		//throw new ApiException(512, errorMessage);
		//		return (false, message);
		//	}

		//	if (eskaadOrderDraftItem != null)
		//	{
		//		if (eskaadOrderDraftItem.CreatedById != User.Id)
		//		{
		//			eskaadOrderDraftItem.UpdatedById = User.Id;
		//		}
		//		eskaadOrderDraftItem.UpdatedDate = now;
		//		eskaadOrderDraftItem.Quantity = quantity;
		//		await dbContext.SaveChangesAsync();
		//		message = $"پیش سفارش با کد {eskaadOrderDraftItem.EskaadCode} از پیش در لیست " +
		//			$"ثبت شده است! تعداد درخواستی به {eskaadOrderDraftItem.Quantity} تغییر کرد.";
		//		return (true, message);
		//	}

		//	eskaadOrderDraftItem = new EskaadOrderDraft()
		//	{
		//		EskaadCode = code,
		//		Quantity = quantity,
		//		CreatedById = User.Id,
		//		CreatedDate = now,
		//		CreatedDatePersian = todayPersian,
		//		OrderIsSealed = false
		//	};
		//	await dbContext.EskaadOrderDrafts.AddAsync(eskaadOrderDraftItem);
		//	await dbContext.SaveChangesAsync();
		//	message = $"پیش سفارش با کد {eskaadOrderDraftItem.EskaadCode} با " +
		//		$"تعداد {eskaadOrderDraftItem.Quantity} به لیست اضافه شد.";
		//	return (true, message);
		//}


		public async Task<(bool, string)> CallDeleteOrderDraftItem(string token, long draftItemId)
		{
			httpClient.DefaultRequestHeaders.Add("Authorization", "bearer " + token);

			var requestBody = new StringContent(
				JsonSerializer.Serialize(new { id = draftItemId }),
				Encoding.UTF8, 
				"application/json");

			var response = await httpClient.PostAsync("DeleteOrderDraftItem", requestBody);
			response.EnsureSuccessStatusCode();
			using var responseStream = await response.Content.ReadAsStreamAsync();
			return (await JsonSerializer.DeserializeAsync<(bool, string)>(responseStream));
		}

		public async Task<(bool, string)> CallPlaceOrdersForTodayOnEskad(string token)
		{
			httpClient.DefaultRequestHeaders.Add("Authorization", "bearer " + token);

			var requestBody = new StringContent(
				JsonSerializer.Serialize(new { id = 0 }),	// This is meaningless! It's just here to fill an empty place, just for the heck of it!
				Encoding.UTF8,
				"application/json");

			var response = await httpClient.PostAsync("PlaceEskaadOrdersForToday", null);
			response.EnsureSuccessStatusCode();
			using var responseStream = await response.Content.ReadAsStreamAsync();
			return await JsonSerializer.DeserializeAsync<(bool, string)>(responseStream);
		}

		public async Task<IQueryable<EskaadSalesModel>> CallGetEskadSalesData(string token, string dateFilter = "")
		{
			httpClient.DefaultRequestHeaders.Add("Authorization", "bearer " + token);
			var response = await httpClient.GetAsync("GetEskaadSalesData");
			response.EnsureSuccessStatusCode();
			using var responseStream = await response.Content.ReadAsStreamAsync();
			return (await JsonSerializer.DeserializeAsync<IEnumerable<EskaadSalesModel>>(responseStream)).AsQueryable();
		}
	}
}
