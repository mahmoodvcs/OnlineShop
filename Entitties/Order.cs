using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MahtaKala.Entities
{
    [Display(Name = "سفارش")]
    public class Order
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderState State { get; set; }
        public IList<OrderItem> Items { get; set; } = new List<OrderItem>();
        public UserAddress Address { get; set; }
        public long? AddressId { get; set; }
        public string TrackNo { get; set; }
        public string DelivererNo { get; set; }
        public DateTime? SendDate { get; set; }
        public DateTime? ApproximateDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
    }
    public enum OrderState
    {
        [Display(Name ="اولیه")]
        Initial = 0,
        [Display(Name ="نهایی شده")]
        CheckedOut = 1,
        [Display(Name ="پرداخت شده")]
        Paid = 2,
        [Display(Name ="ارسال شده")]
        Sent = 3,
        [Display(Name ="تحویل شده")]
        Delivered = 4,
        [Display(Name ="لغو شده")]
        Canceled = 5
    }
}
