using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MahtaKala.Entities
{
    public class Delivery
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string Request { get; set; }
        public long UserId { get; set; }
        public long SellerId { get; set; }
        public Seller Seller { get; set; }
        public string PackKey { get; set; }
        public string OrderItemIds { get; set; }
        [StringLength(100)]
        public string TrackNo { get; set; }
    }
}
