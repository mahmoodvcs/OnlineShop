using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Messages
{
    public static partial class Messages
    {
        public static class Order
        {
            public const string OrderItemDoesNotBelongToOrder = "قلم سبد خرید متعلق به سفارش جاری نیست. لطفا سبد خرید را بروزرسانی کنید";
            public const string EmptyCart = "سبد خرید خالی است";
        }
    }
}
