using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MahtaKala.Entities
{
    public class ProductCategory
    {
        [Key]
        public long ProductId { get; set; }
        [Key]
        public long CategoryId { get; set; }
        public Product Product { get; set; }
        public Category Category { get; set; }

    }
}
