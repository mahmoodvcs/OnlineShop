using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace EskadDataBringer.Models
{
	public class B2BContext : DbContext
	{
		public virtual DbSet<Merchandise> Merchandise { get; set; }

		public virtual DbSet<Sales> Sales { get; set; }

		public B2BContext()
		{ }

		public B2BContext(DbContextOptions<B2BContext> options) : base(options)
		{ }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity(delegate (EntityTypeBuilder<Merchandise> entity)
			{
				entity.Property((Merchandise e) => e.Id).HasColumnName("ID");
				entity.Property((Merchandise e) => e.Code).HasMaxLength(7);
				entity.Property((Merchandise e) => e.Name).HasMaxLength(50);
				entity.Property((Merchandise e) => e.Place).HasMaxLength(5);
				entity.Property((Merchandise e) => e.Unit).HasMaxLength(10);
				entity.Property((Merchandise e) => e.Validation).HasMaxLength(10);
				entity.Property((Merchandise e) => e.Brand).HasMaxLength(25);
				entity.Property((Merchandise e) => e.Group).HasMaxLength(50);
				entity.Property((Merchandise e) => e.ExpireDate).HasColumnName("expiredate").HasMaxLength(12);
				entity.Property((Merchandise e) => e.ConsumerPrice).HasColumnName("consumerprice").HasMaxLength(12);
				entity.Property((Merchandise e) => e.InstallmentPrice).HasColumnName("installmentprice");
				entity.Property((Merchandise e) => e.NumOfInstallment).HasColumnName("numofinstallment");
			});
			modelBuilder.Entity(delegate (EntityTypeBuilder<Sales> entity)
			{
				entity.Property((Sales e) => e.Id).HasColumnName("ID");
				entity.Property((Sales e) => e.Code).HasMaxLength(7);
				entity.Property((Sales e) => e.Date).HasMaxLength(12);
				entity.Property((Sales e) => e.EskadBankCode).HasColumnName("Eskad_Bank_Code").HasMaxLength(10);
				entity.Property((Sales e) => e.MahtaCountBefore).HasColumnName("Mahta_Count_Before");
				entity.Property((Sales e) => e.MahtaFactor).HasColumnName("Mahta_Factor").HasMaxLength(10);
				entity.Property((Sales e) => e.MahtaFactorTotal).HasColumnName("Mahta_Factor_Total");
				entity.Property((Sales e) => e.Place).HasMaxLength(4);
				entity.Property((Sales e) => e.SaleCount).HasColumnName("Sale_Count");
				entity.Property((Sales e) => e.SalePrice).HasColumnName("Sale_Price");
				entity.Property((Sales e) => e.Transact).HasMaxLength(15);
				entity.Property((Sales e) => e.Validation).HasColumnName("validation").HasMaxLength(10);
			});

		}
	}
}
