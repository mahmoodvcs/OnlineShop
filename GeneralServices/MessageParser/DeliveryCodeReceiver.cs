using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MahtaKala.GeneralServices.MessageParser
{
	public class DeliveryCodeReceiver : IDeliveryCodeReceiver
	{
		public DeliveryCodeReceiver(DataContext db)
		{
			this.db = db;
		}
		private readonly DataContext db;
		public (bool, string) CheckReceivedCode(ReceivedSMS receivedSMS)
		{
			var order = db.Orders.FirstOrDefault(x => x.TrackNo.ToLower().Equals(receivedSMS.Message.ToLower()));
			if (order == null)
				return (false, "خطا! سفارشی با این کد در سیستم ثبت نشده است.");
			if (order.State == OrderState.Paid || order.State == OrderState.Sent)
			{
				order.State = OrderState.Delivered;
				db.SaveChanges();
				// TODO: فراخوانی عملیات تسهیم
				return (true, "");
			}
			if (order.State == OrderState.Delivered)
			{
				return (false, "خطا! این کد قبلاً دریافت شده است.");
			}
			if (order.State == OrderState.Initial || order.State == OrderState.CheckedOut)
			{
				return (false, "خطا! هزینه ی سفارش پرداخت نشده است.");
			}
			//if (order.State == OrderState.Canceled)
			// If the code reaches here, it means the only remaining state is "Canceled",
			// unless, the OrderState enum gets modified, in which case we should check for 
			// the newly-added states and do the appropriate action here.

			return (false, "خطا! این سفارش قبلاً لغو شده است.");
		}
	}
}
