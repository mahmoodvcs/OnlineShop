using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MahtaKala.Entities
{
    [Display(Name = "قلم سبد خرید")]
    public class OrderItem
    {
        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public ProductPrice ProductPrice { get; set; }
        public long ProductPriceId { get; set; }

        public long OrderId { get; set; }
        public Order Order { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal FinalPrice { get; set; }

        public IList<CharacteristicValue> CharacteristicValues { get; set; }

        public OrderItemState State { get; set; }
    }

    public enum OrderItemState
    {
        [Display(Name ="اولیه")]
        None,
        [Display(Name ="بسته بندی شد")]
        Packed,
        [Display(Name ="ارسال شد")]
        Sent,
        [Display(Name ="تحویل شد")]
        Delivered
    }
}
