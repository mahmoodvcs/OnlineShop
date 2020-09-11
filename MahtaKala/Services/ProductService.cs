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
            return db.Products.Where(a => a.Published).OrderBy(a => !a.Disabled && a.Prices.Any(p => p.DiscountPrice > 0) ? 0 : 1);
        }

    }
}
