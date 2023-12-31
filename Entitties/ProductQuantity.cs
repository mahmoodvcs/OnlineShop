﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MahtaKala.Entities
{
    public class ProductQuantity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public Product Product { get; set; }
        public long ProductId { get; set; }
        public IList<CharacteristicValue> CharacteristicValues { get; set; }
        public int Quantity { get; set; }
    }
}
