using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;

namespace Unicorn
{
    public enum PersianDayOfWeek
    {
        Saturday = 0,
        Sunday = 1,
        Monday = 2,
        Tuesday = 3,
        Wednesday = 4,
        Thursday = 5,
        Friday = 6,
    }
    /// <summary>
    /// مشخصات تاريخ شمسي را نگه مي دارد.
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(PersianDateTimeTypeConverter))]
    public class PersianDateTime : IComparable<PersianDateTime>, IEquatable<PersianDateTime>
    {
        public static readonly string[] PersianDayNames = new string[] { "شنبه", "يكشنبه", "دوشنبه", "سه شنبه", "چهارشنبه", "پنج شنبه", "جمعه" };
        public static readonly string[] PersianMonthNames = { "فروردین" ,"ارديبهشت","خرداد","تير","مرداد","شهریور"
                                                                ,"مهر","آبان","آذر","دی","بهمن", "اسفند"};

        public PersianDateTime()
            : this(DateTime.Now)
        {
        }
        public PersianDateTime(int year, int month, int day)
        {
            this.year = year;
            this.month = month;
            this.day = day;
            hour = 0;
            minute = 0;
            second = 0;
            millisecond = 0;
        }
        public PersianDateTime(int year, int month, int day, int hour, int minute, int second)
        {
            this.year = year;
            this.month = month;
            this.day = day;
            this.hour = hour;
            this.minute = minute;
            this.second = second;
            millisecond = 0;
        }
        public PersianDateTime(int year, int month, int day, int hour, int minute, int second, int milliseconds)
        {
            this.year = year;
            this.month = month;
            this.day = day;
            this.hour = hour;
            this.minute = minute;
            this.second = second;
            millisecond = milliseconds;
        }

        public PersianDateTime(DateTime d)
            : this(PersianDateTimeConverter.MiladiToShamsi(d))
        {
        }

        public PersianDateTime(PersianDateTime d)
            : this(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second, d.Millisecond)
        {

        }

        /* public string PersianDayOfWeek
         {
             get
             {
                 return persianDayNames[(int)DayOfWeek];
             }
         }*/

        private int year, month, day, hour,
            minute, second, millisecond;

        //public bool IsShamsi
        //{
        //    get
        //    {
        //        if (year < 1700)
        //            return true;
        //        return false;
        //    }
        //}

        public int Year
        {
            get { return year; }
            set
            {
                year = value;
                if (value > 0 && value < 9)
                    year += 1380;
                else if (value > 10 && value < 99)
                    year += 1300;
                else if (value > 100 && value < 999)
                    year += 1000;
                else if (value > 1000 && value < 9999)
                    year += 0;
                else
                    throw new Exception("مقدار سال = " + value + "نامعتبر است.");
            }
        }

        public int Month
        {
            get { return month; }
            set { month = value; }
        }
        public int Day
        {
            get { return day; }
            set { day = value; }
        }
        public int Hour
        {
            get { return hour; }
            set
            {
                hour = value;
                if (hour >= 24)
                    hour = 0;
            }
        }
        public int Minute
        {
            get { return minute; }
            set { minute = value; }
        }
        public int Second
        {
            get { return second; }
            set { second = value; }
        }
        public int Millisecond
        {
            get { return millisecond; }
            set { millisecond = value; }
        }
        public long Ticks
        {
            get { return ((DateTime)this).Ticks; }
        }

        public static PersianDateTime Now
        {
            get
            {
                return PersianDateTimeConverter.MiladiToShamsi(DateTime.Now);
            }
        }
        /* public DayOfWeek DayOfWeek
         {
             get { return ((System.DateTime)this).DayOfWeek; }
         }*/
        public PersianDayOfWeek DayOfWeek
        {
            get
            {
                int dayOfWeekIndex = ((int)((DateTime)this).DayOfWeek + 1) % 7;
                return (PersianDayOfWeek)dayOfWeekIndex;
            }
        }

        public int DayOfYear
        {
            get { return ((System.DateTime)this).DayOfYear; }
        }

        public override string ToString()
        {
            //return year.ToString() + "/" + month.ToString() + '/' + day.ToString() + ' '
            //    + hour.ToString() + ':' + minute.ToString() + ':' + second.ToString();
            return ToString("yyyy/MM/dd hh:mm:ss");
        }

        public string ToShortDateString()
        {
            return ToString("yyyy/MM/dd");
        }

        public string ToLongDateString()
        {
            return String.Format("{0} {1} {2} {3}", PersianDayNames[(int)DayOfWeek], day, PersianMonthNames[month], year);
        }
        public string ToMediumDateString()
        {
            return PersianDayNames[(int)DayOfWeek] + " " + ToShortDateString();
        }
        public string ToMediumDateTimeString()
        {
            return ToMediumDateString() + " " + ToShortTimeString();
        }

        public string ToShortTimeString()
        {
            return ToString("hh:mm:ss");
        }

        public string ToLongTimeString()
        {
            return ToString("hh:mm:ss");
        }

        public string ToShortDateTimeString()
        {
            return ToShortDateString() + " " + ToShortTimeString();
        }
        public string ToString(string format)
        {
            if (format == "G")
                return ToString();
            //return year.ToString() + "/" + month.ToString() + '/' + day.ToString() + ' '
            //    + hour.ToString() + ':' + minute.ToString() + ':' + second.ToString();
            else if (format == "F")
                return ToString("yyyy-MM-dd hh-mm-ss");
            //else if (format.IndexOf("yy") != -1)
            //{
            //    return
            string s = format.Replace("yyyy", year.ToString())
                    .Replace("yy", (year % 100).ToString("00"))
                    .Replace("MM", month.ToString("00"))
                    .Replace("dd", day.ToString("00"))
                    .Replace("hh", hour.ToString("00"))
                    .Replace("mm", minute.ToString("00"))
                    .Replace("ss", second.ToString("00"));
            //}
            //else
            //    return ToString();
            if (s == "")
                throw new Exception("Invalid format '" + format + "'");
            return s;
        }

        public static bool TryParse(string input, out PersianDateTime dateTime)
        {
            dateTime = null;
            try
            {
                dateTime = Parse(input);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static PersianDateTime Parse(string input)
        {
            //براي تاريخ درست كار مي كند و براي ساعت بايد تكميل گردد.
            int y = 0, m = 1, d = 1, ho = 0, mi = 0, se = 0;
            string s = input.Trim();
            string[] ss = s.Split(' ');
            string time = "";
            if (ss.Length >= 2)
            {
                s = ss[0];
                //رشته ساعت كه بايد Parse گردد
                time = string.Join(" ", ss, 1, ss.Length - 1);
                ParseTime(time, out ho, out mi, out se);
            }
            else if (s.IndexOf(':') > 0)
            {
                PersianDateTime p = new PersianDateTime();
                ParseTime(s, out p.hour, out p.minute, out p.second);
                return p;
            }
            ss = s.Split('/', '\\', '-');
            if (ss.Length > 3)
                throw new ArgumentException("Input string is not in a correct format.");
            if(ss.Length == 1 && s.Length == 8)
            {
                ss = new string[3];
                ss[0] = s.Substring(0, 4);
                ss[1] = s.Substring(4, 2);
                ss[2] = s.Substring(6, 2);
            }
            y = ParseInt(ss[0]);
            if (ss.Length > 1)
                m = ParseInt(ss[1]);
            if (ss.Length > 2)
                d = ParseInt(ss[2]);
            if (d > 31)
                if (y < 31)
                {
                    int t = y;
                    y = d;
                    d = t;
                }
                else
                    throw new ArgumentException("Input string is not in a correct format.");
            if (y < 100)
                y += 1300;
            if (y > 1800)
                return (PersianDateTime)System.DateTime.Parse(input);

            return new PersianDateTime(y, m, d, ho, mi, se);

        }

        public static bool TryParseTime(string time, out TimeSpan timeSpan)
        {
            timeSpan = new TimeSpan(0);
            var ss = time.Split(':');
            if (ss.Length == 1 && time.Length == 6)
            {
                ss = new string[3];
                ss[0] = time.Substring(0, 2);
                ss[1] = time.Substring(2, 2);
                ss[2] = time.Substring(4, 2);
            }
            else if (ss.Length != 3)
                return false;
            if (!int.TryParse(ss[0], out var h))
                return false;
            if (!int.TryParse(ss[1], out var m))
                return false;
            if (!int.TryParse(ss[2], out var s))
                return false;
            timeSpan = new TimeSpan(h, m, s);
            return true;
        }
        public static bool ParseTime(string text, out int hour, out int minute, out int seconds)
        {
            try
            {
                DateTime dt = DateTime.Parse(text);
                hour = dt.Hour;
                minute = dt.Minute;
                seconds = dt.Second;
                return true;
            }
            catch
            {
            }
            seconds = 0;
            minute = 0;
            hour = 0;
            string[] ss = text.Split(' ');
            string s = ss[0];
            string[] parts = s.Split(':');
            if (parts.Length < 2)
                return false;
            try
            {
                hour = ParseInt(parts[0]);
                minute = ParseInt(parts[1]);
                if (parts.Length > 2)
                    seconds = ParseInt(parts[2]);
            }
            catch
            {
                return false;
            }
            if (ss.Length > 1)
            {
                s = ss[1].Trim().ToLower();
                if (s.StartsWith("p") || s.StartsWith("ب"))
                    hour += 12;
            }
            return true;
        }

        public int CompareTo(PersianDateTime other)
        {
            if (this.year < other.year)
                return -1;
            else if (this.year > other.year)
                return 1;
            if (this.month < other.month)
                return -1;
            else if (this.month > other.month)
                return 1;
            if (this.day < other.day)
                return -1;
            else if (this.day > other.day)
                return 1;
            if (this.hour < other.hour)
                return -1;
            else if (this.hour > other.hour)
                return 1;
            if (this.minute < other.minute)
                return -1;
            else if (this.minute > other.minute)
                return 1;
            if (this.second < other.second)
                return -1;
            else if (this.second > other.second)
                return 1;
            if (this.millisecond < other.millisecond)
                return -1;
            else if (this.millisecond > other.millisecond)
                return 1;
            return 0;
        }

        public bool Equals(PersianDateTime other)
        {
            return CompareTo(other) == 0;
        }

        #region Operator overloading
        public static explicit operator DateTime(PersianDateTime t)
        {
            return PersianDateTimeConverter.ShamsiToMiladi(t);
        }

        public static implicit operator PersianDateTime(DateTime t)
        {
            return PersianDateTimeConverter.MiladiToShamsi(t);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PersianDateTime))
            {
                throw new ArgumentException("Parameter is not of type PersinaDateTime.", "obj");
            }
            return Equals((PersianDateTime)obj);
        }

        public static bool operator ==(PersianDateTime leftHandValue, PersianDateTime rightHandValue)
        {
            return leftHandValue.Equals(rightHandValue);
        }
        public static bool operator !=(PersianDateTime leftHandValue, PersianDateTime rightHandValue)
        {
            return !leftHandValue.Equals(rightHandValue);
        }
        public static bool operator <(PersianDateTime leftHandValue, PersianDateTime rightHandValue)
        {
            return leftHandValue.CompareTo(rightHandValue) < 0;
        }
        public static bool operator >(PersianDateTime leftHandValue, PersianDateTime rightHandValue)
        {
            return leftHandValue.CompareTo(rightHandValue) > 0;
        }
        public static bool operator <=(PersianDateTime leftHandValue, PersianDateTime rightHandValue)
        {
            return leftHandValue.CompareTo(rightHandValue) <= 0;
        }
        public static bool operator >=(PersianDateTime leftHandValue, PersianDateTime rightHandValue)
        {
            return leftHandValue.CompareTo(rightHandValue) >= 0;
        }
        #endregion Operator overloading

        public long ToInt64()
        {
            return (long)(((DateTime)this) - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }
        public static PersianDateTime FromInt64(long value)
        {
            return (PersianDateTime)(new DateTime(1970, 1, 1) + TimeSpan.FromMilliseconds(value));
        }
        public static int ParseInt(string s)
        {
            string EnglishNumbers = "";
            for (int i = 0; i < s.Length; i++)
            {
                EnglishNumbers += char.GetNumericValue(s, i);
            }
            return Convert.ToInt32(EnglishNumbers);
        }

    }

    public class PersianDateTimeTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string) || sourceType == typeof(DateTime) || sourceType == typeof(long))
                return true;
            return false;
        }
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return CanConvertFrom(context, destinationType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is DateTime)
                return PersianDateTimeConverter.MiladiToShamsi((DateTime)value);
            else if (value is string)
                return PersianDateTime.Parse((string)value);
            else if (value is long)
                return PersianDateTime.FromInt64((long)value);
            throw new ArgumentException("Parameter is not of type PersinaDateTime.", "obj");
        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var p = (PersianDateTime)value;
            if (destinationType == typeof(DateTime))
                return PersianDateTimeConverter.ShamsiToMiladi(p);
            else if (destinationType == typeof(string))
                return p.ToString();
            else if (destinationType == typeof(long))
                return p.ToInt64();
            throw new ArgumentException("Type are not compatible");
        }

    }

}
