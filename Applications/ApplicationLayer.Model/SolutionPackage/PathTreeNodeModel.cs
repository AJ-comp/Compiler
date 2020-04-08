using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ApplicationLayer.Models.SolutionPackage
{
    public abstract class PathTreeNodeModel : TreeNodeModel
    {
        [XmlIgnore]
        public string Path { get; set; }

        [XmlIgnore]
        public string FileName { get; set; }

        [XmlIgnore]
        public bool IsAbsolute { get; set; }

        [XmlIgnore]
        public string PathWithFileName => System.IO.Path.Combine(Path, FileName);

        public PathTreeNodeModel(string path, string name, bool isAbsolute = false)
        {
            this.Path = path;
            this.FileName = name;
            this.IsAbsolute = isAbsolute;
        }

        public override bool Equals(object obj)
        {
            return obj is PathTreeNodeModel model &&
                   IsAbsolute == model.IsAbsolute &&
                   PathWithFileName == model.PathWithFileName;
        }

        public override int GetHashCode()
        {
            int hashCode = -1097576096;
            hashCode = hashCode * -1521134295 + IsAbsolute.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PathWithFileName);
            return hashCode;
        }
    }
}
