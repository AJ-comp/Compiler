using System.Collections.Specialized;

namespace ApplicationLayer.Common.Helpers
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
    }
}
