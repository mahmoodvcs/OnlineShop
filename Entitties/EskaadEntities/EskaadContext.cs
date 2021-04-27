using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MahtaKala.Entities.EskaadEntities
{
    public partial class EskaadContext : DbContext
    {
        public EskaadContext()
        {
        }

        public EskaadContext(DbContextOptions<EskaadContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Merchandise> Merchandise { get; set; }
        public virtual DbSet<Sales> Sales { get; set; }
        public virtual DbSet<UpdateTime> UpdateTime { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    if (!optionsBuilder.IsConfigured)
        //    {
        //        optionsBuilder.UseSqlServer("Server=Remote.Eskad.ir;Database=Mahta;User ID=Mahta-Eskad;Password=@Mahta@&1399!;MultipleActiveResultSets=true", x => x.UseNetTopologySuite());
        //    }
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Merchandise>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code).HasMaxLength(7);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.Place).HasMaxLength(5);

                entity.Property(e => e.Unit).HasMaxLength(10);

                entity.Property(e => e.Validation).HasMaxLength(10);
            });

            modelBuilder.Entity<Sales>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Code).HasMaxLength(7);

                entity.Property(e => e.Date).HasMaxLength(12);

                entity.Property(e => e.EskadBankCode)
                    .HasColumnName("Eskad_Bank_Code")
                    .HasMaxLength(10);

                entity.Property(e => e.MahtaCountBefore).HasColumnName("Mahta_Count_Before");

                entity.Property(e => e.MahtaFactor)
                    .HasColumnName("Mahta_Factor")
                    .HasMaxLength(10);

                entity.Property(e => e.MahtaFactorTotal).HasColumnName("Mahta_Factor_Total");

                entity.Property(e => e.Place).HasMaxLength(4);

                entity.Property(e => e.SaleCount).HasColumnName("Sale_Count");

                entity.Property(e => e.SalePrice).HasColumnName("Sale_Price");

                entity.Property(e => e.Transact).HasMaxLength(15);

                entity.Property(e => e.Validation)
                    .HasColumnName("validation")
                    .HasMaxLength(10);
                entity.Property(e => e.IsInstallmentSale).HasColumnName("is_installment_sell");
                entity.Property(e => e.BAgentName).HasColumnName("BAgent_Name");
            });

            modelBuilder.Entity<UpdateTime>(entity =>
            {
                entity.HasKey(e => e.LastUpdate);

                entity.Property(e => e.LastUpdate).HasMaxLength(100);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
