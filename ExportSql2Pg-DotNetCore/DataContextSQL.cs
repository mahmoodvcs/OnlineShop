using ExportSql2Pg_DotNetCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ExportSql2Pg_DotNetCore
{
    internal class DataContextSQL : MahtaContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        { 
            optionsBuilder.UseSqlServer("Server=Mahta.Eskad.ir;Database=Mahta;uid=sa;pwd=@Mahta@&1399!");
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }
    }
}
