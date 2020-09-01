using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models
{
    public class BuyHistoryModel
    {
        public long Id { get; set; }
        public string Date { get; set; }
        public long Price { get; set; }
        public string Customer { get; set; }
    }
}
