using ApplicationLayer.Common.Helpers;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ApplicationLayer.Models.SolutionPackage
{
    public class FilterFileTreeNodeModel
    {
        public string FilterPath { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;

        [XmlIgnore]
        public FilterTreeNodeModel FilterDataTree
        {
            get
            {
                FilterTreeNodeModel result = null;
                FilterTreeNodeModel curTree = result;

                if (FilterPath.Length == 0) return result;

                string path = FilterPath + "\\";
                while (true)
                {
                    var root = PathHelper.GetRootDirectory(path);
                    path = PathHelper.GetDirectoryPathExceptRoot(path);
                    if (root.Length > 0)
                    {
                        var treeToAdd = new FilterTreeNodeModel(root);

                        if (curTree == null)
                        {
                            result = treeToAdd;
                            curTree = result;
                        }
                        else curTree.AddFilter(treeToAdd);
                        curTree = treeToAdd;
                    }
                    else if (FileName.Length > 0)
                    {
                        var treeToAdd = FileTreeNodeModel.CreateFileTreeNodeModel(Path, FileName);

                        curTree.AddFile(treeToAdd);
                        break;
                    }
                    else break;
                }

                return result;
            }
        }

        [XmlIgnore]
        public FileTreeNodeModel NotFilterDataTree
        {
            get
            {
                if (FilterPath.Length > 0) return null;
                if (FileName.Length == 0) return null;

                return FileTreeNodeModel.CreateFileTreeNodeModel(Path, FileName);
            }
        }

        public FilterFileTreeNodeModel() { }

        public FilterFileTreeNodeModel(string filterPath, string path, string filename)
        {
            FilterPath = filterPath;
            Path = path;
            FileName = filename;
        }

        public override string ToString()
            => string.Format("filter path : {0}, real path : {1}, filename : {2}", FilterPath, Path, FileName);

        public override bool Equals(object obj)
        {
            return obj is FilterFileTreeNodeModel model &&
                   FilterPath == model.FilterPath &&
                   Path == model.Path &&
                   FileName == model.FileName;
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(FilterPath, Path, FileName);
        }
    }
}
