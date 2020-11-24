using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        [StringLength(100)]
        public string PSPReferenceNumber { get; set; }
        public PaymentState State { get; set; }
        public SourceUsedForPayment PaymentSourceUsed { get; set; }

        public bool IsPayable => State == PaymentState.Registerd || State == PaymentState.SentToBank;
    }

    public enum PaymentState
    {
        [Description("آغاز فرایند پرداخت")]
        Registerd,
        [Description("ارسال به بانک")]
        SentToBank,
        [Description("پرداخت تأیید نشده")]
        PaidNotVerified,
        [Description("ناموفق")]
        Failed,
        [Description("پرداخت موفق")]
        Succeeded,
    }

    public enum SourceUsedForPayment
    { 
        [Description("وب اپ از طریق مرورگر")]
        WebSite = 0,
        [Description("اپلیکیشن موبایل روی اندروید یا آی او اس")]
        MobileApp = 1,
        [Description("نامعلوم")]
        Unrecognized = 10
    }
}
