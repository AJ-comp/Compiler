using ApplicationLayer.Common.Helpers;
using ApplicationLayer.Models.SolutionPackage.MiniCPackage;
using Parse.BackEnd.Target;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ApplicationLayer.Models.SolutionPackage
{
    public class TreeNodeCreator
    {
        public static FileTreeNodeModel CreateFileTreeNodeModel(string realFilePath, string parentPath, string fileName)
        {
            FileTreeNodeModel result = null;

            if (PathHelper.ComparePath(realFilePath, parentPath) == false) return result;

            string path = string.Empty;
            if (realFilePath.Length != parentPath.Length)
                path = realFilePath.Substring(realFilePath.IndexOf(parentPath) + parentPath.Length + 1);

            result = new FileTreeNodeModel(path, fileName);

            return result;
        }

        public static FilterTreeNodeModel CreateFilterTreeNodeModel(TreeNodeModel parent, string filterName)
        {
            if (parent == null) return null;

            int index = 1;
            while(true)
            {
                bool bDuplicate = false;
                filterName += index++;

                Parallel.ForEach(parent.Children, (child, loopOption) =>
                {
                    if (child is PathTreeNodeModel)
                    {
                        var pathNode = child as PathTreeNodeModel;
                        if (pathNode.FileName == filterName)
                        {
                            bDuplicate = true;
                            loopOption.Stop();
                        }
                    }
                    else if (child is FilterTreeNodeModel)
                    {
                        var pathNode = child as FilterTreeNodeModel;
                        if (pathNode.FilterName == filterName)
                        {
                            bDuplicate = true;
                            loopOption.Stop();
                        }
                    }
                });

                if (bDuplicate == false) break;
            }

            return new FilterTreeNodeModel(filterName);
        }

        public static ProjectTreeNodeModel CreateProjectTreeNodeModel(string path, string fileName, Target target)
        {
            ProjectTreeNodeModel result = null;

            if (Path.GetExtension(fileName) == "." + LanguageExtensions.MiniC + "proj")
                result = new MiniCProjectTreeNodeModel(path, fileName, target);

            return result;
        }

        public static Type GetType(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            var typeString = (extension.Contains("proj")) ?
                extension.Substring(1, extension.IndexOf("proj") - 1) : extension.Substring(1);

            Type result = typeof(ProjectTreeNodeModel);
            if (typeString == LanguageExtensions.MiniC)
                result = typeof(MiniCProjectTreeNodeModel);

            return result;
        }
    }
}
