using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure
{
    public static class ProductExtentions
    {
        public static bool ShowBuyButton(this Product product)
        {
            return product.Price?.DiscountPrice > 0 && (product.Status == ProductStatus.Available || product.Status == ProductStatus.CantBuy);
        }
    }
}
