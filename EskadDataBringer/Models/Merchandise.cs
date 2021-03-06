using System;
using System.Collections.Generic;
using System.Text;

namespace EskadDataBringer.Models
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

		public string Brand { get; set; }
		public string Group { get; set; }
		public double InstallmentPrice { get; set; }
		public int NumOfInstallment { get; set; }
		public string ExpireDate { get; set; }
		public double Rate { get; set; }
		public byte B2B { get; set; }
		public string ConsumerPrice { get; set; }
	}
}
