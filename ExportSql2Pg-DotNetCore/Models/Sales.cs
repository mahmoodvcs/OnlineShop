using System;
using System.Collections.Generic;
using System.Text;

namespace ExportSql2Pg_DotNetCore.Models
{
    public class Sales
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public double SaleCount { get; set; }

        public string Place { get; set; }

        public string Date { get; set; }

        public string MahtaFactor { get; set; }

        public string Transact { get; set; }

        public string EskadBankCode { get; set; }

        public double SalePrice { get; set; }

        public double MahtaFactorTotal { get; set; }

        public double MahtaCountBefore { get; set; }

        public byte Flag { get; set; }

        public string Validation { get; set; }
    }
}
