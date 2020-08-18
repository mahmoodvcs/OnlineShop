using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MahtaKala.Entities
{
    public class Basket
    {
        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public Product Product { get; set; }
        public long ProductId { get; set; }

        public DateTime Date { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        [StringLength(255)]
        public string CharacteristicName { get; set; }
        [StringLength(255)]
        public string CharacteristicValue { get; set; }
    }
}
