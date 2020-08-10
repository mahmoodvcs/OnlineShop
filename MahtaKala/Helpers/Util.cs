using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
