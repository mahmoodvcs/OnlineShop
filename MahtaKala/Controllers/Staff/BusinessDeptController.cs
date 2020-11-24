using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using MahtaKala.ActionFilter;
using MahtaKala.Entities;
using MahtaKala.Entities.EskaadEntities;
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
		public IActionResult GetEskaadMerchandiseDataSource([DataSourceRequest] DataSourceRequest request)
		{
			var ourProducts = db.Products.Include(x => x.Quantities)
				.Where(x => x.SellerId == ESKAAD_SELLER_ID).ToList();
			var eskaadMerchandise = eskaadDb.Merchandise.ToList();
			var merchandiseModels = new List<EskaadMerchandiseModel>();
			List<string> processedProductCodes = new List<string>();
			Dictionary<string, long> processedProductIds = new Dictionary<string, long>();
			foreach (var eskaadItem in eskaadMerchandise)
			{
				if (eskaadItem.Code.Length < 7)
					throw new Exception("");    // TODO: appropriate exception handling
				if (eskaadItem.Code.Length > 8)
				{
					throw new Exception("");    // TODO: appropriate exception handling
												//throw new Exception(string.Format(
												//	"ESKAAD code invalid! The code in Eskaad is supposed to be 8 or 9 digits, but it has {0} digits (the code is \"{1}\")",
												//	eskaadItem.Code.Length, eskaadItem.Code));
				}
				EskaadMerchandiseModel newItem = new EskaadMerchandiseModel(eskaadItem);
				string mahta_code = eskaadItem.Code.Substring(eskaadItem.Code.Length - 6, 6);
				var ourRespectiveProduct = ourProducts.Where(x => x.Code.Equals(mahta_code)).FirstOrDefault();
				if (ourRespectiveProduct != null)
				{
					newItem.SetMahtaValues(ourRespectiveProduct);
					//processedProductCodes.Add(ourRespectiveProduct.Code);
					if (processedProductIds.ContainsKey(ourRespectiveProduct.Code))
					{
						// TODO: Exception handling!
					}
					processedProductIds.Add(ourRespectiveProduct.Code, ourRespectiveProduct.Id);
				}
				newItem.SetItemProirity();
				merchandiseModels.Add(newItem);
			}
			foreach (var product in ourProducts)
			{
				if (processedProductIds.ContainsKey(product.Code))
				{
					if (processedProductIds[product.Code] == product.Id)
						continue;
					else
					{
						// This means we have two products with different Ids and identical codes!
						// TODO: Inform the proper authorities to take action against the devil's wrong-doings! This will not stand!
					}
				}
				EskaadMerchandiseModel newItem = new EskaadMerchandiseModel(product);
				newItem.SetItemProirity();
				merchandiseModels.Add(newItem);
			}
			return KendoJson(merchandiseModels.AsQueryable().ToDataSourceResult(request));
		}

	}
}
