using System;

namespace MahtaKala.Entities.Security
{
    public static class SecurityStamp
    {
        public static string Generate()
        {
            var guid = Guid.NewGuid();
            var base64 = Convert.ToBase64String(guid.ToByteArray());
            return base64.Substring(0, 22);
        }
    }
}