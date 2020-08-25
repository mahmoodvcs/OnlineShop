using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;

namespace MahtaKala.Entities
{
    public class ProductPrice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonIgnore]
        public long Id { get; set; }
        [JsonIgnore]
        public Product Product { get; set; }
        [JsonIgnore]
        public long ProductId { get; set; }
        public IList<CharacteristicValue> CharacteristicValues { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountPrice { get; set; }
    }
}
