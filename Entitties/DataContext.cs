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
using Microsoft.EntityFrameworkCore.Infrastructure;
using EFSecondLevelCache.Core;
using EFSecondLevelCache.Core.Contracts;
using System.Threading.Tasks;
using System.Threading;

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
                .WithMany(p => p.ProductCategories)
                .HasForeignKey(bc => bc.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProductCategory>()
                .HasOne(pc => pc.Category)
                .WithMany(c => c.ProductCategories)
                .HasForeignKey(pc => pc.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductTag>()
                .HasKey(bc => new { bc.ProductId, bc.TagId});
            modelBuilder.Entity<ProductTag>()
                .HasOne(pc => pc.Product)
                .WithMany(p => p.Tags)
                .HasForeignKey(bc => bc.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProductTag>()
                .HasOne(pc => pc.Tag)
                .WithMany(c => c.ProductTags)
                .HasForeignKey(pc => pc.TagId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>().HasOne(a => a.ProductPrice).WithMany(a => a.OrderItems).OnDelete(DeleteBehavior.Restrict);
        }

        protected virtual bool UseCaching => true;

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

        #region Share
        //public DbSet<PaymentParty> PaymentParties { get; set; }
        //public DbSet<ProductPaymentParty> ProductPaymentParties { get; set; }
        #endregion Share

        public DbSet<Tag> Tags { get; set; }
        public DbSet<ProductTag> ProductTags { get; set; }



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

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            if (UseCaching)
            {
                var changedEntityNames = this.GetChangedEntityNames();

                this.ChangeTracker.AutoDetectChangesEnabled = false; // for performance reasons, to avoid calling DetectChanges() again.
                var result = base.SaveChanges(acceptAllChangesOnSuccess);
                this.ChangeTracker.AutoDetectChangesEnabled = true;

                this.GetService<IEFCacheServiceProvider>().InvalidateCacheDependencies(changedEntityNames);

                return result;
            }
            else
                return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            if (UseCaching)
            {
                var changedEntityNames = this.GetChangedEntityNames();

                this.ChangeTracker.AutoDetectChangesEnabled = false; // for performance reasons, to avoid calling DetectChanges() again.
                var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
                this.ChangeTracker.AutoDetectChangesEnabled = true;

                this.GetService<IEFCacheServiceProvider>().InvalidateCacheDependencies(changedEntityNames);

                return result;
            }
            else
            {
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }
        }
    }
}
