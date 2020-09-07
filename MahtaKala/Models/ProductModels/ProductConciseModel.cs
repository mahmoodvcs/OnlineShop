﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MahtaKala.Models.ProductModels
{
    public class ProductConciseModel
    {
        public long Id { get; set; }

        public string Title { get; set; }
        public string Thubmnail { get; set; }
        public string Category { get; set; }
        public string Brand { get; set; }
        public decimal? Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        [JsonIgnore]
        public bool Disabled { get; set; }
    }
}
