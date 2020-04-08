using System.Collections.Generic;
using System.Xml.Serialization;

namespace ApplicationLayer.Models
{
    public class PathInfo
    {
        public string Type { get; set; } = string.Empty;
        public string Path { get; set; }
        public string FileName { get; set; }
        public bool IsAbsolute { get; set; }

        [XmlIgnore]
        public string FullPath => System.IO.Path.Combine(Path, FileName);

        public PathInfo() { }

        public PathInfo(string path, bool isAbsolute)
        {
            Path = path;
            IsAbsolute = isAbsolute;
        }

        public PathInfo(string path, string fileName, string type, bool isAbsolute) : this(path, isAbsolute)
        {
            Type = type;
            FileName = fileName;
        }

        public override string ToString() => string.Format("[{0},{1}]", this.Path, this.IsAbsolute);

        public override bool Equals(object obj)
        {
            return obj is PathInfo info &&
                   Type == info.Type &&
                   FullPath == info.FullPath;
        }

        public override int GetHashCode()
        {
            int hashCode = 2078297872;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Type);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FullPath);
            return hashCode;
        }
    }
}
