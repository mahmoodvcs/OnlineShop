using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ExportSql2Pg_DotNetCore.Models
{
    public class MahtaContext : DbContext
    {
        public MahtaContext()
        {
        }

        public MahtaContext(DbContextOptions<MahtaContext> options)
          : base((DbContextOptions)options)
        {
        }

        public virtual DbSet<Merchandise> Merchandise { get; set; }

        public virtual DbSet<Sales> Sales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Merchandise>((Action<EntityTypeBuilder<ExportSql2Pg_DotNetCore.Models.Merchandise>>)(entity =>
            {
                entity.Property<long>((Expression<Func<Merchandise, long>>)(e => e.Id)).HasColumnName<long>("ID");
                entity.Property<string>((Expression<Func<Merchandise, string>>)(e => e.Code)).HasMaxLength(7);
                entity.Property<string>((Expression<Func<Merchandise, string>>)(e => e.Name)).HasMaxLength(50);
                entity.Property<string>((Expression<Func<Merchandise, string>>)(e => e.Place)).HasMaxLength(5);
                entity.Property<string>((Expression<Func<Merchandise, string>>)(e => e.Unit)).HasMaxLength(10);
                entity.Property<string>((Expression<Func<Merchandise, string>>)(e => e.Validation)).HasMaxLength(10);
            }));
            modelBuilder.Entity<Sales>((Action<EntityTypeBuilder<Sales>>)(entity =>
            {
                entity.Property<long>((Expression<Func<Sales, long>>)(e => e.Id)).HasColumnName<long>("ID");
                entity.Property<string>((Expression<Func<Sales, string>>)(e => e.Code)).HasMaxLength(7);
                entity.Property<string>((Expression<Func<Sales, string>>)(e => e.Date)).HasMaxLength(12);
                entity.Property<string>((Expression<Func<Sales, string>>)(e => e.EskadBankCode)).HasColumnName<string>("Eskad_Bank_Code").HasMaxLength(10);
                entity.Property<double>((Expression<Func<Sales, double>>)(e => e.MahtaCountBefore)).HasColumnName<double>("Mahta_Count_Before");
                entity.Property<string>((Expression<Func<Sales, string>>)(e => e.MahtaFactor)).HasColumnName<string>("Mahta_Factor").HasMaxLength(10);
                entity.Property<double>((Expression<Func<Sales, double>>)(e => e.MahtaFactorTotal)).HasColumnName<double>("Mahta_Factor_Total");
                entity.Property<string>((Expression<Func<Sales, string>>)(e => e.Place)).HasMaxLength(4);
                entity.Property<double>((Expression<Func<Sales, double>>)(e => e.SaleCount)).HasColumnName<double>("Sale_Count");
                entity.Property<double>((Expression<Func<Sales, double>>)(e => e.SalePrice)).HasColumnName<double>("Sale_Price");
                entity.Property<string>((Expression<Func<Sales, string>>)(e => e.Transact)).HasMaxLength(15);
                entity.Property<string>((Expression<Func<Sales, string>>)(e => e.Validation)).HasColumnName<string>("validation").HasMaxLength(10);
            }));
        }
    }
}
