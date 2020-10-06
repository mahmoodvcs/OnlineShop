using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.CustomerModels
{
    public class OrderItemModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public decimal FinalPrice { get; set; }
        public string DeliveryTrackNo { get; set; }

    }
}
