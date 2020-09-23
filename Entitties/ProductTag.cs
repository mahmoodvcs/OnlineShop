using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MahtaKala.Entities
{
    public class ProductTag
    {
        [Key]
        public long ProductId { get; set; }
        [Key]
        public long TagId { get; set; }
        public Product Product { get; set; }
        public Tag Tag { get; set; }
    }
}
