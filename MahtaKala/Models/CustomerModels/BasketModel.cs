using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.CustomerModels
{
    public class BasketModel
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public DateTime Date { get; set; }
        public long Qty { get; set; }
        public decimal Price { get; set; }
    }
}
