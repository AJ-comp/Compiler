using System.Collections.Generic;

namespace ApplicationLayer.Models
{
    public class PathInfo
    {
        public string Path { get; set; }
        public bool IsAbsolute { get; set; }

        public PathInfo() { }

        public PathInfo(string path, bool isAbsolute)
        {
            Path = path;
            IsAbsolute = isAbsolute;
        }

        public override bool Equals(object obj)
        {
            var info = obj as PathInfo;
            return info != null &&
                   Path == info.Path;
        }

        public override int GetHashCode()
        {
            return 467214278 + EqualityComparer<string>.Default.GetHashCode(Path);
        }

        public override string ToString() => string.Format("[{0},{1}]", this.Path, this.IsAbsolute);
    }
}
