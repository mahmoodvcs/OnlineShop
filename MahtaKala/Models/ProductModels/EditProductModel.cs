using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.ProductModels
{
    public class EditProductModel : Product
    {
        public EditProductModel()
        {
            Characteristics = new List<Characteristic>();
            ProductCategories = new List<ProductCategory>();
            PriceCoefficient = 1;
        }

        public EditProductModel(Product p)
            : this()
        {
            Id = p.Id;
            Title = p.Title;
            Description = p.Description;
            ProductCategories = p.ProductCategories;
            BrandId = p.BrandId;
            Brand = p.Brand;
            Characteristics = p.Characteristics;
            Properties = p.Properties;
            ImageList = p.ImageList;
            Thubmnail = p.Thubmnail;
            //SellerIdNotNull = p.SellerId.HasValue ? p.SellerId.Value : 0;
            SellerId = p.SellerId;
            Seller = p.Seller;
            Code = p.Code;
            Prices = p.Prices;
            Quantities = p.Quantities;
            Price = p.Prices?.FirstOrDefault()?.RawPrice ?? 0;
            DiscountPrice = p.Prices?.FirstOrDefault()?.RawDiscountedPrice ?? 0;
            PriceCoefficient = p.Prices?.FirstOrDefault()?.PriceCoefficient ?? 1;
            Status = p.Status;
            Published = p.Published;
            MinBuyQuota = p.MinBuyQuota;
            MaxBuyQuota = p.MaxBuyQuota;
            BuyQuotaDays = p.BuyQuotaDays;
            Tags = p.Tags;
            if (p.Tags != null)
                TagIds = p.Tags.Select(a => a.TagId).ToList();
            BuyLimitations = p.BuyLimitations;
            if (BuyLimitations != null)
                LimitationIds = BuyLimitations.Select(a => a.BuyLimitationId).ToList();
            if (Quantities != null)
                Quantity = Quantities.FirstOrDefault()?.Quantity ?? 0;
        }

        public new decimal Price { get; set; }
        public decimal DiscountPrice { get; set; }
		public decimal? PriceCoefficient { get; set; }
		public int Quantity { get; set; }
		public new long BrandId { get; set; }
        [Required(ErrorMessage = "فیلد {0} اجباری است.")]
		public long SellerIdNotNull 
        { 
            get 
            { 
                if (SellerId.HasValue) 
                    return SellerId.Value;
                return 0;
            } 
            set { SellerId = value; } 
        }

		public List<long> TagIds { get; set; }
        public List<long> LimitationIds { get; set; }
    }
}
