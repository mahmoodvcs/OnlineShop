using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace MahtaKala.GeneralServices
{
    public static partial class ServiceMessages
    {
        public static class Payment
        {
            public const string InvalidBankResponse = "اطلاعات ارسالی از بانک صحیح نمی باشد";

            public const string PaymentIsNotPayable = "تراکنش با اعطلاعات مورد نظر قابل پرداخت نیست.";
        }
    }
}
