using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace MahtaKala.Entities.Security
{
    public static class PasswordHasher
    {
        private const int Pbkdf2IterCount = 1000;
        private const int Pbkdf2SubkeyLength = 32;
        private const int SaltSize = 16;

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null || 
                b == null || 
                a.Length != b.Length)
            {
                return false;
            }

            var flag = true;
            for (var i = 0; i < a.Length; i++)
            {
                flag &= a[i] == b[i];
            }

            return flag;
        }

        public static string Hash(string password)
        {
            byte[] salt;
            byte[] buffer2;

            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            using (var bytes = new Rfc2898DeriveBytes(password, SaltSize, Pbkdf2IterCount))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(Pbkdf2SubkeyLength);
            }

            var dst = new byte[49];
            Buffer.BlockCopy(salt, 0, dst, 1, SaltSize);
            Buffer.BlockCopy(buffer2, 0, dst, 17, Pbkdf2SubkeyLength);

            return Convert.ToBase64String(dst);
        }

        public static bool Verify(string hashedPassword, string password)
        {
            byte[] buffer4;

            if (hashedPassword == null || password == null)
            {
                return false;
            }

            var src = Convert.FromBase64String(hashedPassword);

            if ((src.Length != 49) || (src[0] != 0))
            {
                return false;
            }

            var dst = new byte[SaltSize];
            Buffer.BlockCopy(src, 1, dst, 0, SaltSize);

            var buffer3 = new byte[Pbkdf2SubkeyLength];
            Buffer.BlockCopy(src, 17, buffer3, 0, Pbkdf2SubkeyLength);

            using (var bytes = new Rfc2898DeriveBytes(password, dst, Pbkdf2IterCount))
            {
                buffer4 = bytes.GetBytes(Pbkdf2SubkeyLength);
            }

            return ByteArraysEqual(buffer3, buffer4);
        }
    }
}