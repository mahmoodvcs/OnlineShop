using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using MahtaKala.Entities.EntityConfig;

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
            modelBuilder.ApplyConfiguration(new ProductConfiguration());
        }

        public DbSet<User> Users{ get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserActivationCode> UserActivationCodes { get; set; }
        public DbSet<ProductCategory> Categories { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<UserAddress> Addresses { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<Basket> Baskets { get; set; }
    }
}
