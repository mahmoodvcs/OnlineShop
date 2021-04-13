using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using MahtaKala.Entities;

namespace MahtaKala.Models.Payment
{
	
	public class BackFromPaymentVM : MahtaKala.Entities.Payment
	{
		public string BaseUrl { get; set; }

		public BackFromPaymentVM(MahtaKala.Entities.Payment payment)
		{
			if (payment == null)
				return;
			this.Amount = payment.Amount;
			this.Id = payment.Id;
			this.OrderId = payment.OrderId;
			this.Order = payment.Order;
			this.PaymentSourceUsed = payment.PaymentSourceUsed;
			this.PayToken = payment.PayToken;
			this.PSPReferenceNumber = payment.PSPReferenceNumber;
			this.ReferenceNumber = payment.ReferenceNumber;
			this.RegisterDate = payment.RegisterDate;
			this.State = payment.State;
			this.TrackingNumber = payment.TrackingNumber;
			this.UniqueId = payment.UniqueId;
		}
	}
}
