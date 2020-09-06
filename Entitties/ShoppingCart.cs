using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MahtaKala.Entities
{
    public class ShoppingCart
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string SessionId { get; set; }

        public long? UserId { get; set; }
        public User User { get; set; }

        public long ProductPriceId { get; set; }
        public ProductPrice ProductPrice { get; set; }

        public int Count { get; set; }

        public DateTime DateCreated { get; set; }
    }
}
