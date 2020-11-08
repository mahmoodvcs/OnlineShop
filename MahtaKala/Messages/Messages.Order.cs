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
            public const string DeliveredOTPMessage = "مهتا کالا: کالای شما ارسال شد. کد دریافت کالا: {0}. در هنگام تحویل، این کد را به پیک ارائه دهید";
            public const string ProductDoesNotExistInStore ="کالای {0} در انبار موجود نیست";
            public const string OrderPaymentSuccessMessage = "مهتاکالا: پرداخت انجام شد. کد تحویل: {0}. ارائه کد فوق هنگام تحویل کالای خریداری شده الزامی است. تاریخ تخمینی ارسال کالا: {1}";
         
            public const string CannotAddProduct_DefferentSeller = "امکان افزودن این کالا وجود ندارد.";//فروشنده ی این کالا، با کالاهای قبلی متفاوت است
            public const string CannotAddProduct_NotAvailable = "کالای {0} موجود نیست.";
            public const string CannotAddProduct_MinQuota = "حداقل تعداد خرید محصول {0}، {1} عدد میباشد.";
            public const string CannotAddProduct_MaxQuota = "حداکثر تعداد خرید محصول {0}، {1} عدد میباشد.";
            public const string CannotAddProduct_WrongCity = "محصول {0} فقط در شهر {1} موجود میباشد.";
            public const string CannotAddProduct_WrongProvince = "محصول {0} فقط در استان {1} موجود میباشد.";
            // TODO: This message is NOT confirmed! The text is temporary! Get the real one!
            public const string CannotAddProduct_SparePartsCategoryConflict = "محصولات دسته لوازم یدکی خودرو را نمیتوان با محصولات دیگر دسته ها همزمان خرید.";
        }
    }
}
