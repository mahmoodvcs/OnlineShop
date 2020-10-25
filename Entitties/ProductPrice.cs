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
		public decimal PriceCoefficient { get; set; }
        [Column("price")]
		public decimal RawPrice { get; set; }
        [NotMapped]
		public decimal Price 
        { 
            get 
            {
                return RawPrice * PriceCoefficient;
            } 
            set 
            {
                RawPrice = value;
            } 
        }
        [Column("discount_price")]
		public decimal RawDiscountedPrice { get; set; }
        [NotMapped]
		public decimal DiscountPrice 
        {
            get
            {
                return RawDiscountedPrice * PriceCoefficient;
            }
            set
			{
                RawDiscountedPrice = value;
			}
        }

        public IList<OrderItem> OrderItems { get; set; }

    }
}
