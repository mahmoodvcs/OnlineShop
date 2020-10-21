using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MahtaKala.Entities.Models
{
    public class ProductModel
    {
        public long Id { get; set; }

        public string Title { get; set; }
        public string Thubmnail { get; set; }
        public List<string> Categories { get; set; }
        public List<string> Tags { get; set; }
        public string Brand { get; set; }
        public decimal? Price { get; set; }
        public decimal? DiscountPrice { get; set; }
		public decimal? PriceCoefficient { get; set; }
		public ProductStatus Status { get; set; }
        public bool Published { get; internal set; }
        public string Seller { get; set; }
        public string Code { get; set; }
    }
}
