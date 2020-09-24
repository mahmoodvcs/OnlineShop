using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Services
{
    public class ProductService
    {
        private readonly DataContext db;

        public ProductService(DataContext db)
        {
            this.db = db;
        }
        public IQueryable<Product> ProductsView()
        {
            return from product in db.Products.Where(a => a.Published)
                   join pt in db.ProductTags on product.Id equals pt.ProductId into prodTags
                   from pt in prodTags.DefaultIfEmpty()
                   join tag in db.Tags on pt.TagId equals tag.Id into tags
                   from tag in tags.DefaultIfEmpty()
                   orderby product.Status, tag.Order, product.Prices.FirstOrDefault().DiscountPrice
                   select product;
        }

    }
}
