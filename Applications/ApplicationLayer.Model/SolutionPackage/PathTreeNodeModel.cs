using System.Collections.Generic;
using System.Xml.Serialization;

namespace ApplicationLayer.Models.SolutionPackage
{
    public abstract class PathTreeNodeModel : TreeNodeModel
    {
        private string pathRecentSaved = string.Empty;
        private string filenameRecentSaved = string.Empty;
        private bool isAbsoluteRecentSaved = false;

        [XmlIgnore]
        public string Path { get; set; }

        [XmlIgnore]
        public string FileName { get; set; }

        [XmlIgnore]
        public bool IsAbsolute { get; set; }

        [XmlIgnore]
        public string PathWithFileName => System.IO.Path.Combine(Path, FileName);

        [XmlIgnore]
        public override bool IsChanged
        {
            get
            {
                if (pathRecentSaved != Path) return true;
                if (filenameRecentSaved != FileName) return true;
                if (isAbsoluteRecentSaved != IsAbsolute) return true;

                return false;
            }
        }

        public PathTreeNodeModel(string path, string name, bool isAbsolute = false)
        {
            this.Path = path;
            this.FileName = name;
            this.IsAbsolute = isAbsolute;

            this.SyncWithCurrentValue();
        }

        public override void SyncWithLoadValue()
        {
            this.Path = this.pathRecentSaved;
            this.FileName = this.filenameRecentSaved;
            this.IsAbsolute = this.isAbsoluteRecentSaved;
        }

        public override void SyncWithCurrentValue()
        {
            this.pathRecentSaved = this.Path;
            this.filenameRecentSaved = this.FileName;
            this.isAbsoluteRecentSaved = this.IsAbsolute;
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
