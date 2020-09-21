using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models
{
    public class BuyHistoryModel
    {
        public long Id { get; set; }
        public string CheckOutData { get; set; }
        public string ApproximateDeliveryDate { get; set; }
        public string SendDate { get; set; }
        public string ActualDeliveryDate { get; set; }
        public long TotalPrice { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string State { get; set; }
    }
}
