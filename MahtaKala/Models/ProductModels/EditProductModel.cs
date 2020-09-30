using MahtaKala.Entities;
using System;
using System.Collections.Generic;
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
            SellerId = p.SellerId;
            Seller = p.Seller;
            Code = p.Code;
            Prices = p.Prices;
            Quantities = p.Quantities;
            Price = p.Prices?.FirstOrDefault()?.Price ?? 0;
            DiscountPrice = p.Prices?.FirstOrDefault()?.DiscountPrice ?? 0;
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
        }

        public new decimal Price { get; set; }
        public decimal DiscountPrice { get; set; }

        public List<long> TagIds { get; set; }
        public List<long> LimitationIds { get; set; }
    }
}
