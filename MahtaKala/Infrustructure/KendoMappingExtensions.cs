using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using MahtaKala.Entities.Models;
using MahtaKala.Services;
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
            var productImageService = MyServiceProvider.ResolveService<IProductImageService>();
            return await query.ToDataSourceResultAsync(request, a => new ProductListModel
            {
                Id = a.Id,
                Title = a.Title,
                Thubmnail = productImageService.GetImageUrl(a.Id, a.Thubmnail),
                Categories = string.Join(",", a.Categories),
                Tags = string.Join(",", a.Tags),
                Brand = a.Brand,
                Supplier = a.Supplier,
                Price = a.Price,
                DiscountPrice = a.DiscountPrice,
                PriceCoefficient = a.PriceCoefficient,
                Status = TranslateExtentions.GetTitle(a.Status),
                Published = a.Published,
                Quantity = a.Quantity,
                Code = a.Code,
                Seller = a.Seller
            });
        }
    }
}
