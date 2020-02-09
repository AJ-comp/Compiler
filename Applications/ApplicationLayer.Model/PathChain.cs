using ApplicationLayer.Common.Helpers;
using System.IO;

namespace ApplicationLayer.Models
{
    public class PathChain
    {
        public string Name { get; }

        public PathChain Prev { get; private set; }
        public PathChain Next { get; private set; }
        public int Count
        {
            get
            {
                int result = 1;

                PathChain iter = this;
                while(iter.Next != null)
                {
                    result++;
                    iter = iter.Next;
                }

                return result;
            }
        }

        public bool IsFirst => (Prev == null);
        public bool IsLast => (Next == null);

        public PathChain()
        {
        }

        public PathChain(string name)
        {
            Name = name;
        }

        public static PathChain CreateChain(string path)
        {
            PathChain result = new PathChain();

            bool bFirst = true;
            while (true)
            {
                string dir = PathHelper.GetRootDirectory(path);
                if (string.IsNullOrEmpty(dir)) break;

                if (bFirst)
                {
                    bFirst = false;
                    result = new PathChain(dir);
                }
                else result.Next = new PathChain(dir);
                path = PathHelper.GetDirectoryPathExceptRoot(path);
            }

            return result;
        }

        public static PathChain CreateChainCheckItem(string baseOpath, string dirPath)
        {
            PathChain result = new PathChain();

            string accumDirectory = string.Empty;
            PathChain current = result;

            bool bFirst = true;
            while (true)
            {
                var root = PathHelper.GetRootDirectory(dirPath);
                if (root.Length == 0) break;

                // If real folder not exist then ignore.
                accumDirectory = Path.Combine(accumDirectory, root);
                if (Directory.Exists(Path.Combine(baseOpath, accumDirectory)) == false) break;

                dirPath = PathHelper.GetDirectoryPathExceptRoot(dirPath);

                if (bFirst)
                {
                    bFirst = false;

                    result = new PathChain(root);
                    current = result;
                }
                else
                {
                    PathChain child = new PathChain(root);
                    current.Next = child;
                    current = child;
                }
            }

            return result;
        }
    }
}
