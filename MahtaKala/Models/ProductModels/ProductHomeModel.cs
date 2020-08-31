using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models.ProductModels
{
    public class ProductHomeModel
    {
        public long Id { get; set; }
        public long CategoryId { get; set; }
        public string Title { get; set; }
        public string Thubmnail { get; set; }
        public IList<string> Images { get; set; }
        public string Category { get; set; }
        public string Brand { get; set; }
        public string Description { get; set; }
        public List<ProductPrice> Prices { get; internal set; }
    }
}
