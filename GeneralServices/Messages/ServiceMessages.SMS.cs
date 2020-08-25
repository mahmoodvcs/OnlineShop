using System;
using System.Collections.Generic;
using System.Text;

namespace MahtaKala.GeneralServices
{
    public static partial class ServiceMessages
    {
        public static class SMS
        {
            public const string SendNetworkError = "خطای شبکه هنگام ارسال پیامک";
            public const string SendHttpError = "ارسال پیام با خطا مواجه شد. {0}";

            public const string SendError = "خطا در ارسال پیام. ";
            public static readonly Dictionary<int, string> ServiceErrors = new Dictionary<int, string>{
                {-1, "اطلاعات موجود نمی باشد" },
                { 2 , "خطای احراز هویت" },
                { 3, "شماره کوتاه نامعتبر" },
                { 5, "اعتبار ناکافی" },
                { 8, "تعداد پیام بیش از حد مجاز" },
                { 10, "حساب کاربری غیر فعال است" },
                { 15, "فاصله زمانی بین درخواست ها کمتر از میزان مجاز کاربر است" }
            };

        }
    }
}
