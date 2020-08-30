using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MahtaKala.Entities
{
    [Display(Name = "محصول")]
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [StringLength(255)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Display(Name ="دسته محصول")]
        public long CategoryId { get; set; }
        [Display(Name ="دسته محصول")]
        public Category Category { get; set; }
        [Display(Name ="برند")]
        public long BrandId { get; set; }
        [Display(Name ="برند")]
        public Brand Brand { get; set; }
        public IList<Characteristic> Characteristics { get; set; }
        public IList<KeyValuePair<string, string>> Properties { get; set; }
        public IList<string> ImageList { get; set; }
        public string Thubmnail { get; set; }

        public long? SellerId { get; set; }
        public Seller Seller { get; set; }

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
