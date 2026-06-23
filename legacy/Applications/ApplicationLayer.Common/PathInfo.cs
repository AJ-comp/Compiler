using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace ApplicationLayer.Common
{
    public class PathInfo
    {
        public string Path { get; set; }
        public string FileName { get; set; }

        [XmlIgnore] public bool IsAbsolute => DriveInfo.GetDrives().Any(x => x.Name == Path);

        [XmlIgnore] public string FullPath => System.IO.Path.Combine(Path, FileName);

        public PathInfo() { }

        public PathInfo(string path)
        {
            Path = path;
        }

        public PathInfo(string path, string fileName) : this(path)
        {
            FileName = fileName;
        }

        public override string ToString() => string.Format("[{0},{1}]", this.Path, this.IsAbsolute);

        public override bool Equals(object obj)
        {
            return obj is PathInfo info &&
                   FullPath == info.FullPath;
        }

        public override int GetHashCode()
        {
            int hashCode = 2078297872;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FullPath);
            return hashCode;
        }
    }
}
