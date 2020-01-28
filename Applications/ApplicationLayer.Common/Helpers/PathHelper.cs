using System.IO;
using System.Linq;

namespace ApplicationLayer.Common.Helpers
{
    public class PathHelper
    {
        public static bool ComparePath(string path1, string path2)
        {
            return NormalizePath(path2).Contains(NormalizePath(path1));
        }

        public static string NormalizePath(string path)
        {
            if (path.Trim().Last().Equals(Path.DirectorySeparatorChar))
                return path.Trim().ToLower();


            return $"{path.Trim()}{Path.DirectorySeparatorChar}".ToLower();
        }

        public static bool IsDrivePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;

            string filterPath = Path.GetPathRoot(path);
            return (filterPath.Length > 2 && filterPath.Contains(":"));
        }
    }
}
