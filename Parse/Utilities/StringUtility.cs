using System;
using System.Linq;

namespace Parse.Utilities
{
    public class StringUtility
    {
        private static Random random = new Random();
        public static string RandomString(int length, bool bIncludeDigit)
        {
            string chars = (bIncludeDigit) ? "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" : "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }



}
