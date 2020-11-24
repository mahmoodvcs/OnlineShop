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
        // This Id property is not copied from anywhere, and is not holding any specific data! It's just a necessity 
		// for the table mandated by database design; Since we can not be sure of neither Eskaad_Id's nor Mahta_Id's unicity,
		// we have to introduce our own unique Id as the table's primary key.
		public long Id { get; set; }
		public long Id_Eskaad { get; set; }
		public long ProductId_Mahta { get; set; }
		public string Code_Eskaad { get; set; }
		public string Code_Mahta { get; set; }
		public string Name_Eskaad { get; set; }
		public string Name_Mahta { get; set; }
		public string Unit_Eskaad { get; set; }
        public double Count_Eskaad { get; set; }
		public int Quantity_Mahta { get; set; }
		public int YellowWarningThreshold_Mahta { get; set; }
		public int RedWarningThreshold_Mahta { get; set; }
		// This is Eskaad's storage unit code in which the product resides. And, yes, the naming is hilarious!
		public string Place_Eskaad { get; set; }
        public double Price_Eskaad { get; set; }
        public byte Active_Eskaad { get; set; }
		public ProductStatus Status_Mahta { get; set; }
		public bool IsPublished_Mahta { get; set; }
		public bool PresentInEskaad { get; set; } = false;
		public bool PresentInMahta { get; set; } = false;
		public string Validation_Eskaad { get; set; }
        public byte? Tax_Eskaad { get; set; }
		public DateTime FetchedDate { get; set; }
	}
}
