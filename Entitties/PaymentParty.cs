using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MahtaKala.Entities
{
    public class PaymentParty
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [StringLength(300)]
        public string Name { get; set; }
        [StringLength(100)]
        public string ShabaId { get; set; }
    }
}
