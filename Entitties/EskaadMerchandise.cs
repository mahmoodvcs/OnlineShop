using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MahtaKala.Entities
{
	public class EskaadMerchandise
	{
        // This is Eskaad's storage unit code in which the product resides. And, yes, the naming is hilarious!
        [Key]
        public long Id { get; set; }
        public string Code { get; set; }
		public string Code_Mahta { get; set; }
		public string Name { get; set; }
        public string Unit { get; set; }
        public double Count { get; set; }
        public string Place { get; set; }
        public double Price { get; set; }
        public byte Active { get; set; }
        public string Validation { get; set; }
        public byte? Tax { get; set; }
    }
}
