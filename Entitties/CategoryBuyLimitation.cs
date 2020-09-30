using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MahtaKala.Entities
{
    public class CategoryBuyLimitation
    {
        [Key]
        public long CategoryId { get; set; }
        [Key]
        public long BuyLimitationId { get; set; }
        public Category  Category{ get; set; }
        public BuyLimitation BuyLimitation { get; set; }
    }
}
