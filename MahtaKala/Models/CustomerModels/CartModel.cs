using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.CustomerModels
{
    public class CartModel
    {
        public List<CartItemModel> Items { get; set; }
        public decimal DeliveryPrice { get; set; }
        public decimal TotlaPrice { get; set; }

    }
}
