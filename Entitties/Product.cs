using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MahtaKala.Entities
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [StringLength(255)]
        public string Title { get; set; }

        public string Description { get; set; }

        public long CategoryId { get; set; }
        public ProductCategory Category { get; set; }
        public long BrandId { get; set; }
        public Brand Brand { get; set; }
        public IList<Characteristic> Characteristics { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public IList<string> ImageList { get; set; }
        public string Thubmnail { get; set; }

        public IList<ProductPrice> Prices { get; set; }
        public IList<ProductQuantity> Quantities { get; set; }

        [NotMapped]
        public decimal Price { get; set; }
        [NotMapped]
        public decimal DiscountPrice { get; set; }

    }

    public class Characteristic
    {
        public string Name { get; set; }
        public List<string> Values { get; set; }
    }

}
