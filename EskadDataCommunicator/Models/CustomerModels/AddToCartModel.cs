using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.CustomerModels
{
    public class AddToCartModel
    {
        public long Id { get; set; }
        public long Product_Id { get; set; }
        public int Quantity { get; set; }
        public IList<CharacteristicValue> CharacteristicValues { get; set; }

    }
}
