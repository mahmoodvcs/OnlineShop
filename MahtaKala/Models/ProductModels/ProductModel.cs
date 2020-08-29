using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MahtaKala.Models.ProductModels
{
    public class ProductModel
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public long Category_Id { get; set; }
        public string Category { get; set; }
        public long Brand_Id { get; set; }
        public string Brand { get; set; }
        public IList<Characteristic> Characteristics { get; set; }
        [JsonIgnore]
        public IList<KeyValuePair<string,string>> PropertiesKeyValues { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public IList<string> ImageList { get; set; }
        public string Thubmnail { get; set; }
        public IList<ProductPrice> Prices { get; set; }
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
        public decimal? DiscountPrice { get; internal set; }
    }

    //public class ProductCharacteristicPrice
    //{
    //    public IList<CharacteristicValue> CharacteristicValues { get; set; }
    //    public decimal Price { get; set; }
    //    /// <summary>
    //    /// If null, the quantity is not known
    //    /// </summary>
    //    public int? Quantity { get; set; }
    //}
}
