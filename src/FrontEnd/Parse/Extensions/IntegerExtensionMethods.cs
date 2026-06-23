using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.Extensions
{
    public static class IntegerExtensionMethods
    {
        public static string ToAnyStrings(this int obj, string toString)
        {
            string result = string.Empty;

            for (int i = 0; i < obj; i++) result += toString;

            return result;
        }

        public static string ToAnyStrings(this uint obj, string toString)
        {
            string result = string.Empty;

            for (int i = 0; i < obj; i++) result += toString;

            return result;
        }

        public static string ToAnyStrings(this long obj, string toString)
        {
            string result = string.Empty;

            for (int i = 0; i < obj; i++) result += toString;

            return result;
        }
    }
}
