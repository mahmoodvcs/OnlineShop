using NetTopologySuite.Noding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MahtaKala.Entities.Models
{
    public class ProductListModel
    {
        [Display(Name ="شناسه")]
        public long Id { get; set; }
        [Display(Name = "نام")]
        public string Title { get; set; }
        [Display(Name ="عکس")]
        public string Thubmnail { get; set; }
        [Display(Name = "دسته بندی")]
        public string Categories { get; set; }
        [Display(Name = "تگ ها")]
        public string Tags { get; set; }
        [Display(Name = "برند")]
        public string Brand { get; set; }
        [Display(Name = "تامین کننده")]
        public string Supplier { get; set; }
        [Display(Name = "قیمت")]
        public decimal? Price { get; set; }
        [Display(Name = "قیمت با تخفیف")]
        public decimal? DiscountPrice { get; set; }
        [Display(Name = "ضریب قیمت")]
        public decimal? PriceCoefficient { get; set; }
        [Display(Name = "وضعیت")]
        public string Status { get; set; }
        [Display(Name = "فروشنده")]
        public string Seller { get; set; }
        [Display(Name = "کد محصول")]
        public string Code { get; set; }
        [Display(Name = "منتشر شده")]
        public bool Published { get; set; }
        [Display(Name = "موجودی")]
        public int Quantity { get; set; }
        [Display(Name = "حداقل تعداد خرید")]
        public int? MinBuyQuota { get; set; }
        [Display(Name = "حداکثر تعداد خرید")]
        public int? MaxBuyQuota { get; set; }
        [Display(Name = "بازه ی زمانی اعمال محدودیت  خرید")]
        public int? BuyQuotaDays { get; set; }
    }
}
