using System.Collections.Generic;
using System.Linq;

namespace Parse.Extensions
{
    public static class HashSetExtensionMethods
    {
        public static HashSet<T> Except<T>(this HashSet<T> obj, T target)
        {
            List<T> exceptContents = new List<T>
            {
                target
            };

            return obj.Except(exceptContents).ToHashSet();
        }

        public static HashSet<T> UnionSet<T>(this HashSet<T> obj, HashSet<T> target)
        {
            obj.UnionWith(target);

            return obj;
        }
    }
}
