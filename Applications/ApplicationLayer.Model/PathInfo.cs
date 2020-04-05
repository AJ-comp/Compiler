using System.Collections.Generic;
using System.Xml.Serialization;

namespace ApplicationLayer.Models
{
    public class PathInfo
    {
        public string Type { get; set; }
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
