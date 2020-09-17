using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MahtaKala.Entities
{
    public class ProductPaymentParty
    {
        [Key]
        public long ProductId { get; set; }
        [Key]
        public long PaymentPartyId { get; set; }
        public Product Product { get; set; }
        public PaymentParty PaymentParty { get; set; }
        public float Percent { get; set; }

    }
}
