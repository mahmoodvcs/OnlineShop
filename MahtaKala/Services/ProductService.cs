using MahtaKala.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;
using Z.EntityFramework.Plus;

namespace MahtaKala.Services
{
    public class ProductService
    {
        private readonly DataContext db;
        private readonly IProductImageService productImageService;

        public ProductService(DataContext db, IProductImageService productImageService)
        {
            this.db = db;
            this.productImageService = productImageService;
        }

        public IQueryable<Product> ProductsView(bool includePrices = false, long[] categoryIds = null)
        {
            var prods = db.Products.AsQueryable();
            if (includePrices)
            {
                prods = prods.Include(a => a.Prices);
            }
            if (categoryIds != null && categoryIds.Length == 0)
                categoryIds = null;

            var query = from product in prods.Where(a => a.Published)
                        from cat in product.ProductCategories.Where(a => categoryIds == null || categoryIds.Contains(a.CategoryId))
                        join pt in db.ProductTags on product.Id equals pt.ProductId into prodTags
                        from pt in prodTags.DefaultIfEmpty()
                        join tag in db.Tags on pt.TagId equals tag.Id into tags
                        from tag in tags.DefaultIfEmpty()
                        orderby product.Status, tag.Order, product.Prices.FirstOrDefault().RawDiscountedPrice
                        select product;

            if (categoryIds != null && categoryIds.Length > 0)
            {
                query = query.Where(p => p.ProductCategories.Any(c => categoryIds.Contains(c.CategoryId)));
            }

            return query;

            return from product in query
                   select new Product
                   {
                       Id = product.Id,
                       Title = product.Title,
                       Brand = product.Brand,
                       BrandId = product.BrandId,
                       BuyLimitations = product.BuyLimitations,
                       BuyQuotaDays = product.BuyQuotaDays,
                       Characteristics = product.Characteristics,
                       Code = product.Code,
                       Description = product.Description,
                       ImageList = product.ImageList,
                       MaxBuyQuota = product.MaxBuyQuota,
                       MinBuyQuota = product.MinBuyQuota,
                       PaymentParties = product.PaymentParties,
                       Prices = product.Prices,
                       ProductCategories = product.ProductCategories,
                       Properties = product.Properties,
                       Published = product.Published,
                       Quantities = product.Quantities,
                       Seller = product.Seller,
                       SellerId = product.SellerId,
                       Status = product.Quantities.FirstOrDefault().Quantity == 0 ? ProductStatus.NotAvailable : product.Status,
                       Tags = product.Tags,
                       Thubmnail = product.Thubmnail,
                   };
        }

    }
}
