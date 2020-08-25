using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MahtaKala.Entities
{
    public class Payment
    {
        public Payment()
        {
            UniqueId = Guid.NewGuid().ToString();
        }

        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long OrderId { get; set; }
        public Order Order { get; set; }
        public string PayToken { get; set; }
        public string UniqueId { get; set; }

        public decimal Amount { get; set; }
        public DateTime RegisterDate { get; set; }
        [StringLength(100)]
        public string ReferenceNumber { get; set; }
        [StringLength(100)]
        public string TrackingNumber { get; set; }
        public PaymentState State { get; set; }

        public bool IsPayable => State == PaymentState.Registerd || State == PaymentState.SentToBank;
    }

    public enum PaymentState
    {
        Registerd,
        SentToBank,
        PaidNotVerified,
        Failed,
        Succeeded,
    }
}
