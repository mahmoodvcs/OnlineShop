using MahtaKala.Entities;
using MahtaKala.Entities.EskaadEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.StaffModels
{
	public class EskaadOrderDraftModel : EskaadOrderDraft
	{
		public string ProductTitle { get; set; }
		public long UnitPrice { get; set; }
		public long InStockQuantity { get; set; }

		public EskaadOrderDraftModel()
		{ }
		public EskaadOrderDraftModel(EskaadOrderDraft orderDraft, Merchandise merchandise)
		{
			this.Id = orderDraft.Id;
			this.EskaadCode = orderDraft.EskaadCode;
			this.Quantity = orderDraft.Quantity;
			this.CreatedDate = orderDraft.CreatedDate;
			this.CreatedDatePersian = orderDraft.CreatedDatePersian;
			this.CreatedById = orderDraft.CreatedById;
			this.CreatedBy = orderDraft.CreatedBy;
			this.UpdatedDate = orderDraft.UpdatedDate;
			this.UpdatedById = orderDraft.UpdatedById;
			this.UpdatedBy = orderDraft.UpdatedBy;
			this.OrderIsSealed = orderDraft.OrderIsSealed;

			if (merchandise != null)
			{
				this.ProductTitle = merchandise.Name;
				this.UnitPrice = (long)merchandise.Price;
				this.InStockQuantity = (long)merchandise.Count;
			}
			else
			{
				this.ProductTitle = "---";
				this.UnitPrice = -1;
				this.InStockQuantity = -1;
			}
		}
	}
}
