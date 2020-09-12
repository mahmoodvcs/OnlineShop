using MahtaKala.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExportDataToPG
{
    public class DataContextSQL: DataContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=.\sql2017;Database=Mahtakala;uid=sa;pwd=123");
        }

        protected override bool UseCaching => false;
    }
}
