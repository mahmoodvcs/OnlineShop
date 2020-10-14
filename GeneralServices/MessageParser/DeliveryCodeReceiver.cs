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
				return (false, "Message body doesn't match any of our tracking numbers.");
			if (order.State == OrderState.Paid || order.State == OrderState.Sent)
			{
				order.State = OrderState.Delivered;
				db.SaveChanges();
				return (true, "");
			}
			if (order.State == OrderState.Delivered)
			{
				return (false, "This order has been already delivered before! This is probably not the first time this message has been received!");
			}

			return (false, $"Order was found, but, its state is not what is's supposed to be! Order.State: {Enum.GetName(typeof(OrderState), order.State)}");
		}
	}
}
