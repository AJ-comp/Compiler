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

        /// <summary>
        /// This function returns a root directory.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetRootDirectory(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;

            path = path.Replace("/", "\\");
            var dirs = path.Split(new char[] { '\\' }, System.StringSplitOptions.RemoveEmptyEntries);

            return (dirs.Length > 0) ? dirs[0] : string.Empty;
        }

        /// <summary>
        /// This function returns directory path except for a root directory.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetDirectoryPathExceptRoot(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;

            path = path.Replace("/", "\\");
            var dirs = path.Split(new char[] { '\\' });

            string result = string.Empty;

            bool bFirstDir = true;
            for(int i=0; i<dirs.Length; i++)
            {
                var dir = dirs[i];

                if (dir == string.Empty) result += "\\";
                else if (bFirstDir) bFirstDir = false;
                else
                {
                    result += dir;
                    if (i != dirs.Length - 1) result += "\\";
                }
            }

            return result;
        }

        public static bool IsDrivePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;

            string filterPath = Path.GetPathRoot(path);
            return (filterPath.Length > 2 && filterPath.Contains(":"));
        }
    }
}
