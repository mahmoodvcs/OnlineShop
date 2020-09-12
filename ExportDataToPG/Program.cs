using MahtaKala.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace ExportDataToPG
{
    class Program
    {
        static void Main(string[] args)
        {
            var c1 = new DataContextSQL();
            var c2 = new DataContextPG();
            //var entities = typeof(DataContext).GetProperties().Where(a=>a.PropertyType.BaseType == typeof(DbSet<>));
            //foreach (var entity in entities)
            //{
            //    c1.Entry(c1.GetType().GetProperty(entity.Name).PropertyType)
            //}

            foreach (var brand in c1.Brands)
            {
                c2.Brands.Add(brand);
            }
            foreach (var c in c1.Categories)
            {
                c2.Categories.Add(c);
            }
            foreach (var c in c1.Sellers)
            {
                c2.Sellers.Add(c);
            }
            foreach (var p in c1.Products.Include(a=>a.Seller).Include(a=>a.Prices).Include(a=>a.ProductCategories))
            {
                c2.Products.Add(p);
            }
            foreach (var c in c1.Provinces.Include(a=>a.Cities))
            {
                c2.Provinces.Add(c);
            }
            c2.SaveChanges();
        }

    }
}
