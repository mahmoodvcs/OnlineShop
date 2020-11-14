using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MahtaKala.Entities
{
	public class EskaadMerchandise
	{
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //public long Id_Mahta { get; set; }
		public long Id { get; set; }
		public string Code { get; set; }
		public string Code_Mahta { get; set; }
		public string Name { get; set; }
        public string Unit { get; set; }
        public double Count { get; set; }
        // This is Eskaad's storage unit code in which the product resides. And, yes, the naming is hilarious!
        public string Place { get; set; }
        public double Price { get; set; }
        public byte Active { get; set; }
        public string Validation { get; set; }
        public byte? Tax { get; set; }
		public DateTime FetchedDate { get; set; }
	}
}
