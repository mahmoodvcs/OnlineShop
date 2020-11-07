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
            try
            {
                var result = await query.ToDataSourceResultAsync(request, a => new ProductListModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Thubmnail = productImageService.GetImageUrl(a.Id, a.Thubmnail),
                    Categories = a.Categories.Aggregate("", (curr, cc) => (curr + cc)),//string.Join(",", a.Categories),
                    Tags = string.Join(",", a.Tags),
                    Brand = a.Brand,
                    Supplier = a.Supplier,
                    Price = a.Price,
                    DiscountPrice = a.DiscountPrice,
                    PriceCoefficient = a.PriceCoefficient,
                    Status = TranslateExtentions.GetTitle(a.Status),
                    Published = a.Published,
                    Quantity = a.Quantity,
                    MinBuyQuota = a.MinBuyQuota,
                    MaxBuyQuota = a.MaxBuyQuota,
                    BuyQuotaDays = a.BuyQuotaDays,
                    Code = a.Code,
                    Seller = a.Seller
                });
                return result;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
