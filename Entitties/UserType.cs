using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MahtaKala.Entities
{
    public enum UserType
    {
        [Display(Name = "مشتری")]
        Customer,
        [Display(Name = "ادمین")]
        Admin,
        [Display(Name = "")]
        Staff,
        [Display(Name = "فروشنده")]
        Seller,
        [Display(Name = "پیک")]
        Delivery
    }
}
