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
            public const string ErrorConvertStateToSent = "وضعیت کالا فقط از حالت خریداری شده به حالت ارسال شده قابل تغییر می باشد.";
            public const string ErrorConvertStateToDelivered = "وضعیت کالا فقط از حالت ارسال شده به حالت تحویل داده شده قابل تغییر می باشد.";
            public const string ErrorWrongTrackNo = "شماره پیگیری وارد شده با شماره پیگیری همخوانی ندارد.";
            public const string DeliveredOTPMessage = "کالای شما ارسال شد. کد پیگیری {0{";
        }
    }
}
