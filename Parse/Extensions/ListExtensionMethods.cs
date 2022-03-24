using System.Collections.Generic;
using System.Linq;

namespace Parse.Extensions
{
    public enum PrintType { String, Type, Property, Function }

    public static class ListExtensionMethods
    {
        public static List<T> ToReverseList<T>(this IList<T> obj)
        {
            List<T> result = new List<T>(obj);
            result.Reverse();

            return result;
        }

        public static T SecondLast<T>(this IList<T> obj)
        {
            if (obj.Count <= 1) return default;

            return obj[obj.Count - 2];
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
        public static int GetIndexNearestLessThanValue(this List<int> obj, int value, bool bZeroDiffInclude = false)
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
        public static int GetIndexNearestMoreThanValue(this List<int> obj, int value, bool bZeroDiffInclude = false)
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

        public static void AddExceptNull<T>(this IList<T> obj, T data)
        {
            if (data == null) return;

            obj.Add(data);
        }

        public static void AddRangeExceptNull<T>(this List<T> obj, IEnumerable<T> collection)
        {
            if (collection == null) return;

            obj.AddRange(collection);
        }


        //public static string ItemsString<T>(this IEnumerable<T> obj, ScopeSyntax scopeSyntax, string propertyName = "")
        //{
        //    string result = string.Empty;

        //    result += scopeSyntax.StartSyntax;
        //    result += ItemsString(obj, propertyName);
        //    result += scopeSyntax.EndSyntax;

        //    return result;
        //}

        public static string ItemsString<T>(this IEnumerable<T> obj, string connector = ", ") => ItemsString(obj, PrintType.String, "", connector);

        public static string ItemsString<T>(this IEnumerable<T> obj, PrintType printType, string name = "", string connector = ", ")
        {
            string result = string.Empty;

            foreach (var item in obj)
            {
                string data = string.Empty;

                try
                {
                    data = (printType == PrintType.Type) ? item.GetType().Name
                            : (printType == PrintType.String) ? item.ToString()
                            : (printType == PrintType.Property) ? item.GetType()
                                                                                         .GetProperty(name)
                                                                                         .GetValue(item) as string
                            : (printType == PrintType.Function) ? item.GetType()
                                                                                         .GetMethod(name)
                                                                                         .Name
                            : string.Empty;
                }
                catch
                {
                    data = item.GetType().Name;
                }

                result += data + connector;
            }
            if (obj.Count() > 0) result = result.Substring(0, result.Length - connector.Length); // remove last string ", ";

            return result;
        }
    }
}