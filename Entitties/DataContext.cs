using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using MahtaKala.Entities.EntityConfig;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Linq;
using System.ComponentModel;

namespace MahtaKala.Entities
{
    public class DataContext : DbContext
    {

        public DataContext()
        { }
        public DataContext(DbContextOptions<DataContext> options) :base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            //{
            //    modelBuilder.Entity(entityType.ClrType).ToTable(entityType.ClrType.Name);
            //}

            modelBuilder.ApplyConfiguration(new ProductConfiguration());
            modelBuilder.ApplyConfiguration(new ProductPriceConfiguration());
            modelBuilder.ApplyConfiguration(new ProductQuantityConfiguration());
            modelBuilder.ApplyConfiguration(new OrderItemConfiguration());

            modelBuilder.Entity<ProductCategory>()
                .HasKey(bc => new { bc.ProductId, bc.CategoryId });
            modelBuilder.Entity<ProductCategory>()
                .HasOne(pc => pc.Product)
                .WithMany(p=>p.ProductCategories)
                .HasForeignKey(bc => bc.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProductCategory>()
                .HasOne(pc => pc.Category)
                .WithMany(c=>c.ProductCategories)
                .HasForeignKey(pc => pc.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>().HasOne(a => a.ProductPrice).WithMany(a => a.OrderItems).OnDelete(DeleteBehavior.Restrict);
        }

        public DbSet<User> Users{ get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserActivationCode> UserActivationCodes { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<UserAddress> Addresses { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<ProductQuantity> ProductQuantities { get; set; }
        public DbSet<ProductPrice> ProductPrices { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }

        public DbSet<Seller> Sellers { get; set; }

        #region Get Titles

        private static Dictionary<Type, string> tableTitles = new Dictionary<Type, string>();
        public static string GetEntityTitle<TEntity>()
        {
            return GetEntityTitle(typeof(TEntity));
        }
        public static string GetEntityTitle(Type entityType)
        {
            if (!tableTitles.TryGetValue(entityType, out var name))
            {
                var dispAttr = entityType.GetCustomAttributes<DisplayAttribute>(false).FirstOrDefault();
                if (dispAttr != null)
                    name = dispAttr.Name;
                else
                {
                    var nameAttr = entityType.GetCustomAttributes<DisplayNameAttribute>(false).FirstOrDefault();
                    name = nameAttr != null ? nameAttr.DisplayName : entityType.Name;
                }
                tableTitles.Add(entityType, name);
            }
            return name;
        }

        #endregion Get Titles
    }
}
