using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace ApplicationLayer.Models.SolutionPackage
{
    public abstract class PathTreeNodeModel : TreeNodeModel, IHasableFileNodes
    {
        [XmlIgnore] public string Path { get; set; }
        [XmlIgnore] public string FileName { get; set; }
        [XmlIgnore] public string FileNameWithoutExtension => System.IO.Path.GetFileNameWithoutExtension(FileName);

        [XmlIgnore] public bool IsAbsolute
        {
            get
            {
                if (Path.Length == 0) return false;

                return Directory.Exists(System.IO.Path.GetPathRoot(Path));
            }
        }

        [XmlIgnore] public string PathWithFileName => System.IO.Path.Combine(Path, FileName);

        [XmlIgnore] public abstract string FullPath { get; }
        [XmlIgnore] public abstract bool IsExistFile { get; }

        [XmlIgnore] public IEnumerable<FileTreeNodeModel> AllFileNodes => TreeNodeModelLogics.GetAllFileNodes(this);

        public PathTreeNodeModel(string path, string name)
        {
            this.Path = path;
            this.FileName = name;
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
