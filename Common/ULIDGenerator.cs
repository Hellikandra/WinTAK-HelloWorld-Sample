using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hello_World_Sample.Common
{
    /* In the .NET Framework 4.8, a ULID (Universal Unique Lexicographically Sortable Identifier)
     * is not provided as a built-in feature.
     * Here is a home made implementation of a ULID Generator.
     *  */
    internal class ULIDGenerator
    {
        private static readonly Random random = new Random();
        private const string CrockforBase32 = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

        public static string GenerateULID()
        {
            var milliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var encodedTime = EncodeULIDTime(milliseconds);
            var randomPart = GenerateRandomPart();

            return encodedTime + randomPart;
        }

        private static string EncodeULIDTime(long milliseconds)
        {
            var encodedChars = new char[10];
            for (var i = 9; i >= 0; i--)
            {
                var remainder = (int)(milliseconds % 32);
                encodedChars[i] = CrockforBase32[remainder];
                milliseconds /= 32;
            }
            return new string(encodedChars);
        }

        private static string GenerateRandomPart()
        {
            var randomChars = new char[16];
            for (var i = 0; i < 16; i++)
            {
                randomChars[i] = CrockforBase32[random.Next(0, CrockforBase32.Length)];
            }
            return new string(randomChars);
        }
    }
}
