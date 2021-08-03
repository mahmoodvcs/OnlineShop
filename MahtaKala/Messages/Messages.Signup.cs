using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Messages
{
    public static partial class Messages
    {
        public static class Signup
        {
            public const string PhoneNumberIsNotValid = "شماره تلفن وارد شده معتبر نیست";
            public const string LoginOTPMessage = "کد ورود به فروشگاه سایت من کالا: {0}";
            public const string MaxSignupAttemptReached = "لطفا بعد از گذشت {0} دقیقه و {1} ثانیه مجددا تلاش کنید";
        }
    }
}
