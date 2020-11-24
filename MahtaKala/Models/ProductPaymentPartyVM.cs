using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models
{
	public class ProductPaymentPartyVM : ProductPaymentParty
	{
		public string ProductName 
		{
			get 
			{
				if (Product != null)
					return Product.Title;
				return "";
			}
		}

		public string PaymentPartyName
		{
			get
			{
				if (PaymentParty != null)
					return PaymentParty.Name;
				return "";
			}
		}
		public string PaymentPartyShaba
		{
			get
			{
				if (PaymentParty != null)
					return PaymentParty.ShabaId;
				return "";
			}
		}
		public float PercentBetween0And1
		{
			get
			{
				return Percent / 100;
			}
		}
	}
}
