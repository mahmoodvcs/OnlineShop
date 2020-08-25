using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MahtaKala.Entities
{
    [Display(Name ="سفارش")]
    public class Order
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
        public DateTime OrrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderState State { get; set; }
        public IList<OrderItem> Items { get; set; }
        public UserAddress Address { get; set; }
        public long? AddressId { get; set; }
    }
    public enum OrderState
    {
        Initial,
        CheckedOut,
        Payed,
        Delivered
    }
}
