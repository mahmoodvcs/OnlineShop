using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MahtaKala.Entities
{
    public class PaymentSettlement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [StringLength(100)]
        public string ShabaId { get; set; }
        [StringLength(300)]
        public string Name { get; set; }
        public int Amount { get; set; }
        public DateTime Date { get; set; }
        public long OrderId { get; set; }
        public Order Order { get; set; }
        public long PaymentId { get; set; }
        public Payment Payment { get; set; }
        public PaymentSettlementStatus Status { get; set; }
        public string Response { get; set; }
        public PayFor PayFor { get; set; }
        public long? ItemId { get; set; }
    }

    public enum PaymentSettlementStatus
    {
        Sent,
        Succeeded,
        Failed
    }
    public enum PayFor
    {
        OrderItem,
        Delivery
    }

}
