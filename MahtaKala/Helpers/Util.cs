using MahtaKala.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MahtaKala.Helpers
{
    public static class Util
    {
        public static string NormalizePhoneNumber(string phoneNumber)
        {
            phoneNumber = phoneNumber.Trim();
            if (phoneNumber.StartsWith("+"))
                phoneNumber = phoneNumber.Remove(0, 1);
            if (phoneNumber.StartsWith("989"))
                phoneNumber = phoneNumber.Remove(0, 2);
            else if (phoneNumber.StartsWith("0989"))
                phoneNumber = phoneNumber.Remove(0, 3);
            if (!phoneNumber.StartsWith("0"))
                phoneNumber = "0" + phoneNumber;
            return phoneNumber;
        }

        public static void CheckNationalCode(string nationalCode)
        {
            //در صورتی که کد ملی وارد شده تهی باشد

            if (String.IsNullOrEmpty(nationalCode))
                throw new Exception("لطفا کد ملی را صحیح وارد نمایید");


            //در صورتی که کد ملی وارد شده طولش کمتر از 10 رقم باشد
            if (nationalCode.Length != 10)
                throw new Exception("طول کد ملی باید ده کاراکتر باشد");

            //در صورتی که کد ملی ده رقم عددی نباشد
            var regex = new Regex(@"\d{10}");
            if (!regex.IsMatch(nationalCode))
                throw new Exception("کد ملی تشکیل شده از ده رقم عددی می‌باشد؛ لطفا کد ملی را صحیح وارد نمایید");

            //در صورتی که رقم‌های کد ملی وارد شده یکسان باشد
            var allDigitEqual = new[] { "0000000000", "1111111111", "2222222222", "3333333333", "4444444444", "5555555555", "6666666666", "7777777777", "8888888888", "9999999999" };
            if (allDigitEqual.Contains(nationalCode))
                throw new Exception("کد ملی صحیح نیست");


            //عملیات شرح داده شده در بالا
            var chArray = nationalCode.ToCharArray();
            var num0 = Convert.ToInt32(chArray[0].ToString()) * 10;
            var num2 = Convert.ToInt32(chArray[1].ToString()) * 9;
            var num3 = Convert.ToInt32(chArray[2].ToString()) * 8;
            var num4 = Convert.ToInt32(chArray[3].ToString()) * 7;
            var num5 = Convert.ToInt32(chArray[4].ToString()) * 6;
            var num6 = Convert.ToInt32(chArray[5].ToString()) * 5;
            var num7 = Convert.ToInt32(chArray[6].ToString()) * 4;
            var num8 = Convert.ToInt32(chArray[7].ToString()) * 3;
            var num9 = Convert.ToInt32(chArray[8].ToString()) * 2;
            var a = Convert.ToInt32(chArray[9].ToString());

            var b = (((((((num0 + num2) + num3) + num4) + num5) + num6) + num7) + num8) + num9;
            var c = b % 11;

            if (((c < 2) && (a == c)) || ((c >= 2) && ((11 - c) == a)))
                return;

            throw new Exception("کد ملی صحیح نیست");
        }

        public static string Sub3Number(decimal price)
        {
            string value = price.ToString("0");
            int i = value.Length;
            while (i > 3)
            {
                value = value.Insert(i - 3, ",");
                i -= 3;
            }
            return value;
        }

        public static bool  IsValidEmailaddress(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool IsDiscount(ProductPrice productPrice, out decimal discount)
        {
            discount = 0;
            decimal val = productPrice.Price - productPrice.DiscountPrice;
            if (val > 0 && productPrice.Price > 0)
            {
                discount = Math.Round(val * 100 / productPrice.Price, 0);
                return true;
            }
            return false;
        }

        public static string GetPersianDate(DateTime? d)
        {
            if (d == null)
                return null;
            PersianCalendar pc = new PersianCalendar();
            return $"{pc.GetYear(d.Value)}/{pc.GetMonth(d.Value)}/{pc.GetDayOfMonth(d.Value)} {d.Value.ToString("HH:mm")}";
        }

        public static string GetTimeSpanPersianString(TimeSpan timeSpan)
        {
            List<Tuple<int, string>> spans = new List<Tuple<int, string>>
            {
                new Tuple<int, string>(365, "سال" ),
                new Tuple<int, string>(30, "ماه" ),
                new Tuple<int, string>(1, "روز" ),
            };

            var days = (int)timeSpan.TotalDays;
            List<string> parts = new List<string>();
            foreach (var s in spans)
            {
                var x = days / s.Item1;
                if (x > 0)
                {
                    parts.Add($"{x} {s.Item2}");
                    days = days % x;
                }
            }
            return string.Join(" و ", parts);
        }


    }
}
