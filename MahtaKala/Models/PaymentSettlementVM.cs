using MahtaKala.Entities;
using MahtaKala.Helpers;
using MahtaKala.Infrustructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models
{
	public class PaymentSettlementVM : PaymentSettlement
	{
		public string SettlementDatePersian 
		{
			get 
			{ 
				if (Date == null || Date.Year < 1990 /*WHY?*/) 
					return "";
				return Util.GetPersianDate(Date);
			} 
		}

		public string PayForPersian
		{
			get
			{
				return PayFor.GetEnumDescriptionAttribute();
			}
		}

		public string SettlementStatusPersian
		{
			get
			{
				return Status.GetEnumDescriptionAttribute();
			}
		}

		public string PaymentStatusPersian
		{
			get
			{
				if (Payment == null)
					return "";
				return Payment.State.GetEnumDescriptionAttribute();
			}
		}
	}
}
