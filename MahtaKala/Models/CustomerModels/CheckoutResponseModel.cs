using NetTopologySuite.Index.Strtree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.CustomerModels
{
    public class CheckoutResponseModel
    {
        public long OrderId { get; set; }
        public string PaymentUrl { get; set; }
		public string TimeOfDeliveryMessage { get; set; }
	}
}
