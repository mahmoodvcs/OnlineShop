using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace MahtaKala.Entities
{
    [Display(Name = "محصول")]
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        public string Description { get; set; }

        public IList<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

        [Display(Name ="برند")]
        [Required(ErrorMessage = "فیلد برند باید مقدار داشته باشد.")]
        public long BrandId { get; set; }
        [Display(Name = "برند")]
        public Brand Brand { get; set; }
        public long? SupplierId { get; set; }
        [Display(Name = "تامین کننده")]
        public Supplier Supplier { get; set; }
        public IList<Characteristic> Characteristics { get; set; }
        public IList<KeyValuePair<string, string>> Properties { get; set; } = new List<KeyValuePair<string, string>>();
        public IList<string> ImageList { get; set; }
        public string Thubmnail { get; set; }

        public long? SellerId { get; set; }
        public Seller Seller { get; set; }

        [Display(Name ="کد محصول")]
        [StringLength(200)]
        public string Code { get; set; }

        public IList<ProductPrice> Prices { get; set; } = new List<ProductPrice>();
        public IList<ProductQuantity> Quantities { get; set; } = new List<ProductQuantity>();

        [Display(Name = "وضعیت")]
        public ProductStatus Status { get; set; }
        [Display(Name ="منتشر شده")]
        public bool Published  { get; set; }
        [Display(Name = "حداقل تعداد خرید")]
        public int? MinBuyQuota { get; set; }
        [Display(Name = "حداکثر تعداد خرید")]
        public int? MaxBuyQuota { get; set; }
        [Display(Name = "بازه ی زمانی اعمال محدودیت  خرید")]
        public int? BuyQuotaDays { get; set; }

        public IList<ProductTag> Tags { get; set; }

        public IList<ProductPaymentParty> PaymentParties { get; set; }
        public IList<ProductBuyLimitation> BuyLimitations { get; set; }

        //public bool Deleted { get; set; }


        [NotMapped]
        public ProductPrice Price => Prices?.FirstOrDefault();
    }

    public enum ProductStatus
    {
        [Display(Name ="موجود")]
        Available = 0,
        [Display(Name ="به زودی")]
        CommingSoon = 1,
        [Display(Name ="ناموجود")]
        NotAvailable = 2,
        [Display(Name = "امکان خرید ندارد")]
        CantBuy
    }

    public class Characteristic
    {
        public string Name { get; set; }
        public List<string> Values { get; set; }
    }

}
