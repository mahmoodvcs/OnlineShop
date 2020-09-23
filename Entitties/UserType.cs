using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MahtaKala.Entities
{
    public enum UserType
    {
        [Display(Name = "مشتری")]
        Customer = 0,
        [Display(Name = "ادمین")]
        Admin = 1,
        [Display(Name = "کارمند")]
        Staff = 2,
        [Display(Name = "فروشنده")]
        Seller = 3,
        [Display(Name = "پیک")]
        Delivery = 4
    }
}
