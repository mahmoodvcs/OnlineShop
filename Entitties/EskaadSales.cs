using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MahtaKala.Entities
{
    // This is a table for Eskaad-fiasco-compensation!
    // The Sales table from EskaadEntities is not persistent, i.e., its data will be deleted each time the 
    // order (going to Eskaad from Mahta) is attended to, so, to keep the history of our orders to Eskaad,
    // we have this table, in which we keep every data row we insert in the EskaadEntities.Sales table.
    public class EskaadSales
	{
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        // Since the property Id from Sales table is generated outside our jurisdiction, and, since that table 
        // is emptied periodically, that Id property may NOT be unique in this table ('cause we're not gonna 
        // delete the previous records), so, we're gonna have to have our own unique Id_Mahta property, and the 
        // Id property is not the primary key in this table
        public long Id_Mahta { get; set; }    
		public long Id { get; set; }  // This is the original Id from EskaadEntities.Sales table.
        public string Code { get; set; }
		public string Code_Mahta { get; set; }  // This is the product's code as is saved in Mahta database
		public double SaleCount { get; set; }
        public string Place { get; set; }
        public string Date { get; set; }    // Shamsi date, with format "yyyy/mm/dd"
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
