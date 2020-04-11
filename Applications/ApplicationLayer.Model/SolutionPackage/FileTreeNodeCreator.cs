using ApplicationLayer.Models.SolutionPackage.MiniCPackage;
using Parse.BackEnd.Target;
using System;
using System.IO;

namespace ApplicationLayer.Models.SolutionPackage
{
    public class FileTreeNodeCreator
    {
        public static FileTreeNodeModel CreateFileTreeNodeModel(string path, string fileName)
        {
            FileTreeNodeModel result = null;

            if (Path.GetExtension(fileName) == "." + LanguageExtensions.MiniC)
                result = new MiniCFileTreeNodeModel(path, fileName);

            return result;
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
