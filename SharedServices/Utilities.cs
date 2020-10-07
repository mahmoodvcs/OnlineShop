using System;
using System.Collections.Generic;
using System.Text;

namespace MahtaKala.SharedServices
{
    public class Utilities
    {
        public static DateTime ParseDateTime(string str)
        {
            return (DateTime)Unicorn.PersianDateTime.Parse(str);
        }
    }
}
