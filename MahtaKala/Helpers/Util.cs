using MahtaKala.Entities;
using MahtaKala.Infrustructure.Exceptions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MahtaKala.Helpers
{
    public static class Util
    {
		public const char ZERO_WIDTH_NON_BREAKING_SPACE = '​';
        public const char STANDARD_SPACE_CHARACTER = ' ';
		public static readonly Dictionary<string, string> PersianDaysOfTheWeek = new Dictionary<string, string>()
		    { { "saturday", "شنبه" },
			{ "sunday", "یکشنبه" },
			{ "monday", "دوشنبه" },
			{ "tuesday", "سه شنبه" },
			{ "wednesday", "چهارشنبه" },
			{ "thursday", "پنجشنبه" },
			{ "friday", "جمعه" } };
        public static readonly List<string> PersianDayOfTheWeekByIndex = new List<string>()
            { "یکشنبه", "دوشنبه", "سه شنبه", "چهارشنبه", "پنجشنبه", "جمعه", "شنبه" };

		public static string TrimString(string input)
        {
            return input.Trim().Trim(ZERO_WIDTH_NON_BREAKING_SPACE);
        }

        public static string RemoveExcessWhiteSpaces1(string input)
        {
            //input = input.ToLower();
            var resultBuilder = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            { 
                if(char.IsWhiteSpace(input[i]))
				{
                    if (resultBuilder.Length > 0 && resultBuilder[resultBuilder.Length - 1] != STANDARD_SPACE_CHARACTER)
                    {
                        resultBuilder.Append(STANDARD_SPACE_CHARACTER);
                    }
				}
				else
				{
                    resultBuilder.Append(input[i]);
				}
            }
            return resultBuilder.ToString();
        }

        public static string RemoveExcessWhiteSpaces2(string input)
        {
            //input = input.ToLower();
            var wordList = Regex.Split(input, @"\s{1,}").ToList();
            var resultBuilder = new StringBuilder();
            foreach (var word in wordList)
            {
                if (string.IsNullOrWhiteSpace(word))
                    continue;
                if (resultBuilder.Length > 0)
                    resultBuilder.Append(' ');
                resultBuilder.Append(word);
            }
            return resultBuilder.ToString();
        }

        public static string NormalizeStringForWordWiseLooseComparison(string input)
        {
            string result = ReplaceArabicCharacters(input.ToLower());
            result = RemoveExcessSpacesAndSortWordsForLooseComparison(result);
            return result;
        }

        public static string ReplaceArabicCharacters(string input)
        {
            return input.Replace("ﮎ", "ک").Replace("ﮏ", "ک").Replace("ﮐ", "ک").Replace("ﮑ", "ک").Replace("ك", "ک").Replace("ي", "ی");
        }

        public static string RemoveExcessSpacesAndSortWordsForLooseComparison(string input)
        {
            var words = Regex.Split(input, @"\s{1,}").ToList();
            words = words.Except(words.Where(x => string.IsNullOrWhiteSpace(x))).ToList();
            var wordList = new List<string>();
            foreach (var word in words)
            {
                if (!string.IsNullOrWhiteSpace(word))
                    wordList.Add(word);
            }
            wordList.Sort();
            var resultBuilder = new StringBuilder();
            foreach (var word in wordList)
            {
                if (resultBuilder.Length > 0)
                    resultBuilder.Append(' ');
                resultBuilder.Append(word);
            }
            return resultBuilder.ToString();
        }

        //public static bool AssessSimilarity(string input1, string input2)
        //{ 

        //}

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

            if (String.IsNullOrWhiteSpace(nationalCode))
                throw new ApiException(400, Messages.Messages.UserErrors.NationalCode_Empty );


            //در صورتی که کد ملی وارد شده طولش کمتر از 10 رقم باشد
            if (nationalCode.Length != 10)
                throw new ApiException(400, Messages.Messages.UserErrors.NationalCode_IncorrectLength );

            //در صورتی که کد ملی ده رقم عددی نباشد
            var regex = new Regex(@"\d{10}");
            if (!regex.IsMatch(nationalCode))
                throw new ApiException(400, Messages.Messages.UserErrors.NationalCode_ContainsNonDigits );

            //در صورتی که رقم‌های کد ملی وارد شده یکسان باشد
            var allDigitEqual = new[] { "0000000000", "1111111111", "2222222222", "3333333333", "4444444444", "5555555555", "6666666666", "7777777777", "8888888888", "9999999999" };
            if (allDigitEqual.Contains(nationalCode))
                throw new ApiException(400, Messages.Messages.UserErrors.NationalCode_Incorrect);


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

            throw new ApiException(400, Messages.Messages.UserErrors.NationalCode_Incorrect);
        }


        public static bool IsCheckNationalCode(string nationalCode ,out string msg)
        {
            //در صورتی که کد ملی وارد شده تهی باشد
            msg = string.Empty;
            if (String.IsNullOrWhiteSpace(nationalCode))
            {
                msg = Messages.Messages.UserErrors.NationalCode_Incorrect;
                return false;
            }
               
            //در صورتی که کد ملی وارد شده طولش کمتر از 10 رقم باشد
            if (nationalCode.Length != 10)
            {
                msg = Messages.Messages.UserErrors.NationalCode_IncorrectLength;
                return false;
            }

            //در صورتی که کد ملی ده رقم عددی نباشد
            var regex = new Regex(@"\d{10}");
            if (!regex.IsMatch(nationalCode))
            {
                msg = Messages.Messages.UserErrors.NationalCode_ContainsNonDigits;
                return false;
            }

            //در صورتی که رقم‌های کد ملی وارد شده یکسان باشد
            var allDigitEqual = new[] { "0000000000", "1111111111", "2222222222", "3333333333", "4444444444", "5555555555", "6666666666", "7777777777", "8888888888", "9999999999" };
            if (allDigitEqual.Contains(nationalCode))
            {
                msg = Messages.Messages.UserErrors.NationalCode_Incorrect;
                return false;
            }


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
                return true;
            msg = Messages.Messages.UserErrors.NationalCode_Incorrect;
            return false;
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

        public static bool IsValidEmailaddress(string emailaddress)
        {
            Regex regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            return regex.IsMatch(emailaddress);
        }

        public static bool IsAnyNumberInString(string text)
        {
            Regex regex = new Regex("[0-9]");
            return regex.IsMatch(text);
        }

        public static bool IsNumber(string text)
        {
            long val;
            if (long.TryParse(text, out val))
                return true;
            return false;
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
            return $"{pc.GetYear(d.Value)}/{pc.GetMonth(d.Value)}/{pc.GetDayOfMonth(d.Value)} {d.Value:HH:mm}";
        }

        public static string GetPersianDateRange(DateTime? d, TimeSpan range)
        {
            if (d == null)
                return null;
            PersianCalendar pc = new PersianCalendar();
            var persianDayOfTheWeek = PersianDayOfTheWeekByIndex[(int)pc.GetDayOfWeek(d.Value)];
            return $"{persianDayOfTheWeek} {pc.GetYear(d.Value)}/{pc.GetMonth(d.Value)}/{pc.GetDayOfMonth(d.Value)} - ساعت {d.Value:HH\\:mm} تا {d.Value + range:HH\\:mm}";
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

        public static Color ParseColor(string cssColor)
        {
            cssColor = cssColor.Trim();

            if (cssColor.StartsWith("#"))
            {
                return ColorTranslator.FromHtml(cssColor);
            }
            else if (cssColor.StartsWith("rgb")) //rgb or argb
            {
                int left = cssColor.IndexOf('(');
                int right = cssColor.IndexOf(')');

                if (left < 0 || right < 0)
                    throw new FormatException("rgba format error");
                string noBrackets = cssColor.Substring(left + 1, right - left - 1);

                string[] parts = noBrackets.Split(',');

                int r = int.Parse(parts[0], CultureInfo.InvariantCulture);
                int g = int.Parse(parts[1], CultureInfo.InvariantCulture);
                int b = int.Parse(parts[2], CultureInfo.InvariantCulture);

                if (parts.Length == 3)
                {
                    return Color.FromArgb(r, g, b);
                }
                else if (parts.Length == 4)
                {
                    float a = float.Parse(parts[3], CultureInfo.InvariantCulture);
                    return Color.FromArgb((int)(a * 255), r, g, b);
                }
            }
            throw new FormatException("Not rgb, rgba or hexa color string");
        }

        public static string ChangeNumberBaseFrom10(long sourceNumber, char[] destinationCharSet)
        {
            StringBuilder resultBuilder = new StringBuilder();
            int destinationBase = destinationCharSet.Length;
            do
            {
                resultBuilder.Insert(0, destinationCharSet[sourceNumber % destinationBase]);
                sourceNumber /= destinationBase;
            } 
            while (sourceNumber > 0);

            return resultBuilder.ToString();
        }

        public static long ChangeNumberBaseTo10(string sourceValue, char[] sourceBaseCharSet)
        {
            int sourceBase = sourceBaseCharSet.Length;
            int coef = 1;
            Dictionary<char, int> sourceBaseCharsIndexes = new Dictionary<char, int>();
            for (int i = 0; i < sourceBaseCharSet.Length; i++)
            {
                sourceBaseCharsIndexes[sourceBaseCharSet[i]] = i;
            }
            long result = 0;
            for (int i = sourceValue.Length - 1; i >= 0; i--)
            {
                result += sourceBaseCharsIndexes[sourceValue[i]] * coef;
                coef *= sourceBase;
            }
            return result;
        }

        public static string GetIpAddress(HttpContext context)
        {
            if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
                return context.Request.Headers["X-Forwarded-For"];
            else
                return context.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }


    }
}
