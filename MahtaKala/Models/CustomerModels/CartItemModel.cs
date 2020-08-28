using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.CustomerModels
{
    public class CartItemModel
    {
        public long Id { get; set; }
        public long ProductPrice_Id { get; set; }
        public int Quantity { get; set; }
        public IList<CharacteristicValue> CharacteristicValues { get; set; }
        public string Thumbnail { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
    }
}
