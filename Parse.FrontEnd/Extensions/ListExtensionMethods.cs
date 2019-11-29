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

        public static bool IsEqual<T>(this List<T> obj, List<T> target)
        {
            if (obj.Count != target.Count) return false;

            for (int i = 0; i < obj.Count; i++)
            {
                if (obj[i].Equals(target[i]) == false) return false;
            }

            return true;
        }

        /// <summary>
        /// This function returns the nearest index from the value less than of the values of the obj.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetIndexNearestLessThanValue(this List<int>obj, int value)
        {
            int result = -1;
            if (obj.Count == 0) return result;

            int minDiff = 0xff;
            obj.ForEach(i =>
            {
                if (i <= value)
                {
                    int diff = value - i;
                    if (diff < minDiff)
                    {
                        result = i;
                        minDiff = diff;
                    }
                }
            });

            return result;
        }

        /// <summary>
        /// This function returns the nearest index from the value more than of the values of the obj.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetIndexNearestMoreThanValue(this List<int>obj, int value)
        {
            int result = -1;
            if (obj.Count == 0) return result;

            int minDiff = 0xff;
            obj.ForEach(i =>
            {
                if (i >= value)
                {
                    int diff = i - value;
                    if (diff < minDiff)
                    {
                        result = i;
                        minDiff = diff;
                    }
                }
            });

            return result;
        }
    }
}