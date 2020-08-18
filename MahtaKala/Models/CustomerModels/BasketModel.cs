using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.CustomerModels
{
    public class BasketModel
    {
        public long Id { get; set; }
        public long Product_Id { get; set; }
        public DateTime Date { get; set; }
        public int Qty { get; set; }
        public decimal Price { get; set; }
        public string Characteristic_Name { get; set; }
        public string Characteristic_Value { get; set; }

    }
}
