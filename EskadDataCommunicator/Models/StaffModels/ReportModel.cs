using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.StaffModels
{
    public class ReportModel
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public Decimal TotalPayments { get; set; }

        public List<ChartModel> OrderChart { get; set; }
        public List<ChartModel> SaleChart { get; set; }

    }
}
