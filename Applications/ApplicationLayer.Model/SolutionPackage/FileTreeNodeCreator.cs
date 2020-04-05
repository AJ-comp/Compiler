using ApplicationLayer.Models.SolutionPackage.MiniCPackage;
using System.IO;

namespace ApplicationLayer.Models.SolutionPackage
{
    public class FileTreeNodeCreator
    {
        public static FileTreeNodeModel Create(string path, string fileName)
        {
            FileTreeNodeModel result = null;

            if (Path.GetExtension(fileName) == "." + LanguageExtensions.MiniC)
                result = new MiniCFileTreeNodeModel(path, fileName);

            return result;
        }
    }
}
