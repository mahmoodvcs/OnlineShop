using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure.Extensions
{
    public static class StringExtensions
    {
        public static string ToEnglishNumber(this object Text)
        {
            string str = (string)Text;
            string vInt = "۱۲۳۴۵۶۷۸۹۰";
            char[] mystring = str.ToCharArray(0, str.Length);
            var newStr = string.Empty;
            for (var i = 0; i <= (mystring.Length - 1); i++)
                if (vInt.IndexOf(mystring[i]) == -1)
                    newStr += mystring[i];
                else
                {
                    newStr += char.GetNumericValue(mystring[i]);
                }
            return newStr;
        }
    }
}
