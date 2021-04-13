using System.Collections.Generic;
using System.Collections.Specialized;

namespace AJ.Common.Helpers
{
    public static class StringHelper
    {
        /// <summary>
        /// This function returns the index that matched with the target in the collection.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="target">The target to find.</param>
        /// <returns>If found returns find index, if not found returns -1.</returns>
        public static int FindIndex(this StringCollection collection, string target)
        {
            int result = -1;

            for (int i = 0; i < collection.Count; i++)
            {
                if(collection[i] == target)
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

        public static bool Compare (this StringCollection src, StringCollection target)
        {
            if (src.Count != target.Count) return false;

            foreach(var item in src)
            {
                if (target.Contains(item) == false) return false;
            }

            return true;
        }
    }
}
