using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MahtaKala.Entities
{
    public class Seller
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [StringLength(255)]
        public string Name { get; set; }
        [StringLength(30)]
        public string AccountNumber { get; set; }
        [StringLength(50)]
        public string AccountBankName { get; set; }
    }
}
