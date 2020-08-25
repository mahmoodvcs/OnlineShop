using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MahtaKala.Entities
{
    [Display(Name = "قلم سبد خرید")]
    public class OrderItem
    {
        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public Product Product { get; set; }
        public long ProductId { get; set; }

        public long OrderId { get; set; }
        public Order Order { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public IList<CharacteristicValue> CharacteristicValues { get; set; }

    }
}
