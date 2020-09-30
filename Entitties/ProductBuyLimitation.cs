using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MahtaKala.Entities
{
    public class ProductBuyLimitation
    {
        [Key]
        public long ProductId { get; set; }
        [Key]
        public long BuyLimitationId { get; set; }
        public Product Product { get; set; }
        public BuyLimitation BuyLimitation { get; set; }
    }
}
