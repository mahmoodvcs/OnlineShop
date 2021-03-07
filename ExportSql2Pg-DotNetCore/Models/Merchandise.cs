using System;
using System.Collections.Generic;
using System.Text;

namespace ExportSql2Pg_DotNetCore.Models
{
    public class Merchandise
    {
        private DateTime _cdate;

        public long Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Unit { get; set; }

        public double Count { get; set; }

        public string Place { get; set; }

        public double Price { get; set; }

        public byte? Active { get; set; }

        public string Validation { get; set; }

        public byte? Tax { get; set; }
    }
}
