using System.Collections.Generic;

namespace Parse.Extensions
{
    public static class ListExtensionMethods
    {
        public static List<T> ToReverseList<T>(this List<T> obj)
        {
            List<T> result = new List<T>(obj);
            result.Reverse();

            return result;
        }
    }
}