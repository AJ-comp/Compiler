using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace AJ.Common.Helpers
{
    public static class StringHelper
    {
        /***************************************************************/
        /// <summary>
        /// This function returns the index that matched with the target in the collection.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="target">The target to find.</param>
        /// <returns>If found returns find index, if not found returns -1.</returns>
        /***************************************************************/
        public static int FindIndex(this StringCollection collection, string target)
        {
            int result = -1;

            for (int i = 0; i < collection.Count; i++)
            {
                if (collection[i] == target)
                {
                    result = i;
                    break;
                }
            }

            return result;
        }

        public static string[] ToArray(this StringCollection collection)
        {
            List<string> result = new List<string>();

            foreach (var item in collection) result.Add(item);

            return result.ToArray();
        }

        public static bool Compare(this StringCollection src, StringCollection target)
        {
            if (src.Count != target.Count) return false;

            foreach (var item in src)
            {
                if (target.Contains(item) == false) return false;
            }

            return true;
        }


        /***************************************************************/
        /// <summary>
        /// This function returns a repeated target string
        /// </summary>
        /// <param name="target"></param>
        /// <param name="repeatCount"></param>
        /// <remark>
        /// ex : "]".RepeatString(3) => "]]]"
        /// </remark>
        /// <returns></returns>
        /***************************************************************/
        public static string RepeatString(this string target, int repeatCount)
        {
            string result = string.Empty;

            for (int i = 0; i < repeatCount; i++) result += target;

            return result;
        }


        public static string RandomString(int length, bool bIncludeDigit)
        {
            Random random = new Random();

            string chars = (bIncludeDigit) ? "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" : "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }


        public static IEnumerable<uint> ToIntList(this string data)
        {
            List<uint> result = new List<uint>();

            var items = data.Split(",", StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in items)
                result.Add(Convert.ToUInt32(item));

            return result;
        }


        public static bool LastMatch(this string origin, string target, string separator = ".")
        {
            var index = origin.LastIndexOf(target);
            if (index == -1) return false;

            // ex) origin: System.Linq.Generic
            // target: Generic
            // => origin[index - 1] == '.' so true
            // target: eneric
            // => origin[index - 1] == 'G' so false
            return (index == 0) ? index + target.Length == origin.Length
                                         : index + target.Length == origin.Length && origin[index - 1] == '.';
        }


        public static string AbsolutePath(this string path)
            => Path.IsPathRooted(path) ? path : Path.Combine(Environment.CurrentDirectory, path);

        public static string RelativePathIfContain(this string path, string parentPath)
        {
            var result = path.Replace(@"\\", @"\").Replace("/", @"\");
            parentPath = parentPath.Replace(@"\\", @"\").Replace("/", @"\");

            if (result.Contains(parentPath))
            {
                result = result.Replace(parentPath, "");
                if (result[0] == '/' || result[0] == '\\') result = result.Substring(1);
            }

            return result;
        }
    }
}
