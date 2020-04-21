using System;
using System.Collections.Generic;

namespace Parse.Extensions
{
    public static class ListExtensionMethods
    {
        public static List<T> ToReverseList<T>(this IList<T> obj)
        {
            List<T> result = new List<T>(obj);
            result.Reverse();

            return result;
        }

        public static bool IsEqual<T>(this IList<T> obj, IReadOnlyList<T> target)
        {
            if (obj.Count != target.Count) return false;

            for (int i = 0; i < obj.Count; i++)
            {
                if (obj[i].Equals(target[i]) == false) return false;
            }

            return true;
        }

        public static void RemoveList<T>(this IList<T> obj, List<int> indexesToRemove)
        {
            indexesToRemove.Sort();
            int adjustCounter = 0;
            for (int i = 0; i < indexesToRemove.Count; i++)
            {
                if (i + adjustCounter >= obj.Count) break;

                obj.RemoveAt((i + adjustCounter--));
            }
        }

        /// <summary>
        /// This function returns the nearest index from the value less than of the values of the obj.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        /// <param name="bZeroDiffInclude"></param>
        /// <returns></returns>
        public static int GetIndexNearestLessThanValue(this List<int>obj, int value, bool bZeroDiffInclude=false)
        {
            int result = -1;
            if (obj.Count == 0) return result;

            int minDiff = int.MaxValue;
            obj.ForEach(i =>
            {
                if (i <= value)
                {
                    int diff = value - i;
                    if (diff == 0)
                    {
                        if (bZeroDiffInclude) { result = i; minDiff = diff; }
                    }
                    else if (diff < minDiff) { result = i; minDiff = diff; }
                }
            });

            return result;
        }

        /// <summary>
        /// This function returns the nearest index from the value more than of the values of the obj.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        /// <param name="bZeroDiffInclude"></param>
        /// <returns></returns>
        public static int GetIndexNearestMoreThanValue(this List<int>obj, int value, bool bZeroDiffInclude=false)
        {
            int result = -1;
            if (obj.Count == 0) return result;

            int minDiff = int.MaxValue;
            obj.ForEach(i =>
            {
                if (i >= value)
                {
                    int diff = i - value;
                    if (diff == 0)
                    {
                        if (bZeroDiffInclude) { result = i; minDiff = diff; }
                    }
                    else if (diff < minDiff) { result = i; minDiff = diff; }
                }
            });

            return result;
        }
    }
}