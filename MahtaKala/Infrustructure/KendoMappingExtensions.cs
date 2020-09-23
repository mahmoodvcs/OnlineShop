using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using MahtaKala.Entities.Models;
using MahtaKala.SharedServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure
{
    public static class KendoMappingExtensions
    {
        public static async Task<DataSourceResult> ToListResultAsync(this IQueryable<ProductModel> query, DataSourceRequest request)
        {
            return await query.ToDataSourceResultAsync(request, a => new ProductListModel
            {
                Id = a.Id,
                Title = a.Title,
                Thubmnail = a.Thubmnail,
                Categories = string.Join(",", a.Categories),
                Brand = a.Brand,
                Price = a.Price,
                DiscountPrice = a.DiscountPrice,
                Status = TranslateExtentions.GetTitle(a.Status),
                Published = a.Published,
                Code = a.Code,
                Seller = a.Seller
            });
        }
    }
}
