using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models
{
    public class CharacteristicVM
    {
       public int ProductId { get; set; }
        public List<CharacteristicItemVM> Items { get; set; }
    }

    public class CharacteristicItemVM
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountPrice { get; set; }
    }
}
