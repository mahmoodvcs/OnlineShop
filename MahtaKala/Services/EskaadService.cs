using MahtaKala.Entities;
using MahtaKala.Entities.EskaadEntities;
using MahtaKala.Helpers;
using MahtaKala.Infrustructure.Exceptions;
using MahtaKala.SharedServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Services
{
	public class EskaadService
	{
		private readonly ICurrentUserService currentUserService;
		private readonly ILogger<EskaadService> logger;
		private readonly DataContext dbContext;
		private readonly EskaadContext eskaadDbContext;

		public const long ESKAAD_SELLER_ID = 54;
		public const int MAHTA_STORAGE_NUMBER_IN_ESKAAD = 13;
		public static readonly string[] ESKAAD_AUTHORIZED_USERS = { "katouzian", "mosalli", "ali.d" };

		User User => currentUserService.User;

		public EskaadService(ICurrentUserService currentUserService
							, ILogger<EskaadService> logger
							, DataContext dbContext
							, EskaadContext eskaadContext)
		{
			this.currentUserService = currentUserService;
			this.logger = logger;
			this.dbContext = dbContext;
			this.eskaadDbContext = eskaadContext;
		}

		public IQueryable<Merchandise> GetEskaadMerchandise(bool onlyActive = true, bool removeIfNotInStock = true)
		{
			var merchandise = eskaadDbContext.Merchandise.Where(x => Convert.ToInt32(x.Place) != MAHTA_STORAGE_NUMBER_IN_ESKAAD);
			if (onlyActive)
			{
				merchandise = merchandise.Where(x => x.Active == 1);
			}
			if (removeIfNotInStock)
			{
				merchandise = merchandise.Where(x => x.Count > 0);
			}
			return merchandise;
		}

		public IQueryable<EskaadOrderDraft> GetOrderDraftsForToday()
		{
			var now = DateTime.Now;
			var orderDraftQuery = dbContext.EskaadOrderDrafts.Where(x => x.CreatedDate.Date.Equals(now.Date));
			return orderDraftQuery;
		}

		public async Task<bool> EskaadOrderAlreadyPlacedToday()
		{
			var now = DateTime.Now;
			var todayPersian = Util.GetPersianDate(now, true);
			var todayPersianWithLeadingZeros = Util.GetPersianDate(now, true, true);
			var alreadyDoneToday = await eskaadDbContext.Sales
				.AnyAsync(x => x.Date.Equals(todayPersian) || x.Date.Equals(todayPersianWithLeadingZeros));
			return alreadyDoneToday;
		}

		public async Task<string> AddNewOrderItem(string code, int quantity)
		{
			var now = DateTime.Now;
			var todayPersian = Util.GetPersianDate(now, true, true);
			string message = "";
			if (await dbContext.EskaadOrderDrafts.AnyAsync(x =>
					x.CreatedDate.Date.Equals(now.Date)
					&& x.OrderIsSealed))
			{
				throw new ApiException(512, $"سفارشات امروز ({todayPersian}) بسته شده اند! امکان افزودن سفارش جدید برای امروز وجود ندارد!");
			}
			var eskaadOrderDraftItem = dbContext.EskaadOrderDrafts.Where(x =>
					x.CreatedDate.Equals(now.Date) && x.EskaadCode.Equals(code))
				.FirstOrDefault();
			//if (alreadyExistingOrderDraft != null)
			//{
			//	quantity += alreadyExistingOrderDraft.Quantity;
			//}
			if (!await eskaadDbContext.Merchandise.AnyAsync(x => x.Code.Equals(code) 
					&& Convert.ToInt32(x.Place) != MAHTA_STORAGE_NUMBER_IN_ESKAAD
					&& x.Active == 1))
			{
				message = "";
				//if (await dbContext.EskaadOrderDrafts.AnyAsync(x => x.CreatedDate.Equals(now.Date) && x.EskaadCode.Equals(code)))
				if (eskaadOrderDraftItem != null)
				{
					//var notAvailableAnyMoreEntity = dbContext.EskaadOrderDrafts.Where(x =>
					//		x.CreatedDate.Equals(now.Date) && x.EskaadCode.Equals(code)).First();
					dbContext.EskaadOrderDrafts.Remove(eskaadOrderDraftItem);
					await dbContext.SaveChangesAsync();
					message = $"آیتم از پیش ثبت شده ی امروز با کد {code} نیز از لیست لیست حذف می شود.";
				}
				message = $"کالایی با کد {code} در دیتابیس اسکاد وجود ندارد!" + Environment.NewLine + message;
				//throw new ApiException(512, errorMessage);
				return message;
			}
			var merchandiseItem = await eskaadDbContext.Merchandise.Where(x => x.Code.Equals(code)
					&& Convert.ToInt32(x.Place) != MAHTA_STORAGE_NUMBER_IN_ESKAAD
					&& x.Active == 1)
				.FirstOrDefaultAsync();
			if (merchandiseItem.Count < quantity)
			{
				message = "";
				if (eskaadOrderDraftItem != null)
				{ 
					if (merchandiseItem.Count < eskaadOrderDraftItem.Quantity)
					{
						message = $"آیتم قبلاً ثبت شده ی امروز با کد {code} نیز از لیست حذف می شود - تعداد ثبت شده: {eskaadOrderDraftItem.Quantity}";
						dbContext.EskaadOrderDrafts.Remove(eskaadOrderDraftItem);
						await dbContext.SaveChangesAsync();
					}
					else
					{
						message = $"ولی آیتم قبلاً ثبت شده ی امروز، با تعداد سفارش {eskaadOrderDraftItem.Quantity} در لیست باقی خواهد ماند.";
					}
				}
				message = $"کالای {merchandiseItem.Name} با کد {merchandiseItem.Code} به تعداد کافی در"
					+ $" انبار اسکاد موجود نیست! تعداد موجود: {merchandiseItem.Count} - تعداد مورد نیاز: {quantity}"
					+ message;
				//throw new ApiException(512, errorMessage);
				return message;
			}

			if (eskaadOrderDraftItem != null)
			{
				eskaadOrderDraftItem.UpdatedById = User.Id;
				eskaadOrderDraftItem.Quantity = quantity;
				eskaadOrderDraftItem.UpdatedDate = now;
				await dbContext.SaveChangesAsync();
				message = $"پیش سفارش با کد {eskaadOrderDraftItem.EskaadCode} از پیش در لیست " +
					$"ثبت شده است! تعداد درخواستی به {eskaadOrderDraftItem.Quantity} تغییر کرد.";
				return message;
			}

			eskaadOrderDraftItem = new EskaadOrderDraft()
			{
				EskaadCode = code,
				Quantity = quantity,
				CreatedById = User.Id,
				CreatedDate = now,
				CreatedDatePersian = todayPersian,
				OrderIsSealed = false
			};
			await dbContext.EskaadOrderDrafts.AddAsync(eskaadOrderDraftItem);
			await dbContext.SaveChangesAsync();
			message = $"پیش سفارش با کد {eskaadOrderDraftItem.EskaadCode} با " +
				$"تعداد {eskaadOrderDraftItem.Quantity} به لیست اضافه شد.";
			return message;
		}

		private async Task<string> CreateToday_sFactor()
		{
			var latestSalesItem = await eskaadDbContext.Sales.OrderByDescending(x => x.Date).FirstOrDefaultAsync();
			if (latestSalesItem == null)
				return "1000001";
			var previousMahtaFactor = latestSalesItem.MahtaFactor;
			long numericFactor = Convert.ToInt64(previousMahtaFactor);
			numericFactor++;
			return numericFactor.ToString("0000000");
		}

		public async Task<int> PlaceToday_sOrdersForEskaad()
		{
			var now = DateTime.Now;
			var todayPersian = Util.GetPersianDate(now, true, true);
			if (await dbContext.EskaadOrderDrafts.AnyAsync(x =>
					x.CreatedDate.Date.Equals(now.Date)
					&& x.OrderIsSealed))
			{
				var errorMessage = $"سفارشات امروز ({todayPersian}) بسته شده اند! امکان افزودن سفارش جدید برای امروز وجود ندارد!";
				logger.LogError("EskaadService - PlaceToday_sOrdersForEskaad - " + errorMessage);
				throw new ApiException(512, errorMessage);
			}
			var todaysMahtaFactor = await CreateToday_sFactor();
			var ordersForToday = await dbContext.EskaadOrderDrafts.Where(x => x.CreatedDate.Date.Equals(now.Date)).ToListAsync();
			var finalOrderList = new List<Sales>();
			foreach (var draftItem in ordersForToday)
			{
				var merchandiseCount = await eskaadDbContext.Merchandise.CountAsync(x => x.Code.Equals(draftItem.EskaadCode)
						&& Convert.ToInt32(x.Place) != MAHTA_STORAGE_NUMBER_IN_ESKAAD
						&& x.Active == 1);
				if (merchandiseCount > 1)
				{
					string errorMessage = $"خطا! بیش از یک کالای فعال با کد {draftItem.EskaadCode} در دیتابیس اسکاد وجود دارد! عملیات لغو میشود.";
					logger.LogError("EskaadService - PlaceToday_sOrdersForEskaad - " + errorMessage);
					throw new ApiException(512, errorMessage);
				}
				if (merchandiseCount < 1)
				{
					string errorMessage = $"خطا! هیچ کالای فعالی با کد  {draftItem.EskaadCode} در دیتابیس اسکاد وجود ندارد! عملیات لغو میشود.";
					logger.LogError("EskaadService - PlaceToday_sOrdersForEskaad - " + errorMessage);
					throw new ApiException(512, errorMessage);
				}
				var merchandiseItem = await eskaadDbContext.Merchandise.Where(x => x.Code.Equals(draftItem.EskaadCode)
						&& Convert.ToInt32(x.Place) != MAHTA_STORAGE_NUMBER_IN_ESKAAD
						&& x.Active == 1).SingleAsync();
				if (merchandiseItem.Count < draftItem.Quantity)
				{
					string errorMessage = $"خطا! موجودی کالا با کد {draftItem.EskaadCode} در انبار اسکاد کمتر از تعداد مورد نیاز است! " +
						$"تعداد مورد نیاز: {draftItem.Quantity} - تعداد موجود در انبار اسکاد: {merchandiseItem.Count} - عملیات لغو میشود.";
					logger.LogError("EskaadService - PlaceToday_sOrdersForEskaad - " + errorMessage);
					throw new ApiException(512, errorMessage);
				}
				var newSaleItem = new Sales()
				{ 
					Code = merchandiseItem.Code,
					SaleCount = draftItem.Quantity,
					Date = todayPersian,
					Flag = 0,
					Place = merchandiseItem.Place,
					Validation = merchandiseItem.Validation,
					MahtaFactor = todaysMahtaFactor,
					SalePrice = 0,
					MahtaFactorTotal = 0,
					MahtaCountBefore = 0,
					Transact = null,
					EskadBankCode = null,
				};
				//eskaadDbContext.Sales.Add(newSaleItem);
				finalOrderList.Add(newSaleItem);
				draftItem.OrderIsSealed = true;
			}
			await eskaadDbContext.Sales.AddRangeAsync(finalOrderList);
			int updatedCount = await eskaadDbContext.SaveChangesAsync();
			await dbContext.SaveChangesAsync();
			return updatedCount;
		}
	}
}
