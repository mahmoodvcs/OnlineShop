using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.StaffModels
{
    public class OrderItemModel
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public string Date { get; set; }
        public string Product { get; set; }
        public long ProductId { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
        public long UserId { get; set; }
        public string State { get; set; }
    }
}
