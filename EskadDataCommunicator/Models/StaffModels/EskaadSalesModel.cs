using MahtaKala.Entities.EskaadEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.StaffModels
{
	public class EskaadSalesModel : Sales
	{
		public string ProductTitle { get; set; }
		public EskaadSalesModel(Sales saleItem)
		{
			this.Id = saleItem.Id;
			this.Code = saleItem.Code;
			this.Date = saleItem.Date;
			this.SalePrice = saleItem.SalePrice;
			this.Flag = saleItem.Flag;
			this.SaleCount = saleItem.SaleCount;
			this.Place = saleItem.Place;
			this.EskadBankCode = saleItem.EskadBankCode;
			this.Transact = saleItem.Transact;
			this.Validation = saleItem.Validation;
			this.MahtaFactor = saleItem.MahtaFactor;
			this.MahtaFactorTotal = saleItem.MahtaFactorTotal;
			this.MahtaCountBefore = saleItem.MahtaCountBefore;
		}
	}
}
