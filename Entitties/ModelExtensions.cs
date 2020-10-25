using MahtaKala.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MahtaKala.Entities
{
    public static class ModelExtensionsa
    {
        public static IQueryable<ProductModel> Project(this IQueryable<Product> query)
        {
            return query.Select(a => new ProductModel
            {
                Id = a.Id,
                Brand = a.Brand.Name,
                Categories = a.ProductCategories.Select(c => c.Category.Title).ToList(),
                Tags = a.Tags.Select(c => c.Tag.Name).ToList(),
                Title = a.Title,
                Thubmnail = a.Thubmnail,
                Price = a.Prices.FirstOrDefault().Price,
                DiscountPrice = a.Prices.FirstOrDefault().DiscountPrice,
                Quantity = a.Quantities.FirstOrDefault().Quantity,
                PriceCoefficient = a.Prices.FirstOrDefault().PriceCoefficient,
                Status = a.Status,
                Published = a.Published,
                Seller = a.Seller.Name,
                Code = a.Code
            });
        }

    }
}
