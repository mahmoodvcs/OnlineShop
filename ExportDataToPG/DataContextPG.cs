using MahtaKala.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExportDataToPG
{
    public class DataContextPG : DataContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=MahtaKala;Username=postgres;Password=1").UseSnakeCaseNamingConvention();
        }
        protected override bool UseCaching => false;
    }
}
