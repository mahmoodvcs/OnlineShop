using System;
using System.Collections.Generic;
using System.Text;

namespace MahtaKala.Entities.Models
{
    public class ProductListModel
    {
        public long Id { get; set; }

        public string Title { get; set; }
        public string Thubmnail { get; set; }
        public string Categories { get; set; }
        public string Brand { get; set; }
        public decimal? Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public bool Disabled { get; set; }
        public bool Published { get; internal set; }
    }
}
