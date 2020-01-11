using System.IO;
using System.Xml;
using WpfApp.Models;
using WpfApp.Utilities.GeneratorPackages.ProjectGenerators;
using WpfApp.Utilities.GeneratorPackages.ProjectLoaders;

namespace WpfApp.Utilities
{
    public class SolutionLoader
    {
        public SolutionStruct LoadSolution(string solutionPath, string solutionName)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Path.Combine(solutionPath, solutionName));

            XmlNodeList itemNodes = xmlDoc.SelectNodes("//Solution/Project");

            SolutionStruct result = new SolutionStruct() { FullName = solutionName };

            foreach (XmlNode itemNode in itemNodes)
            {
                XmlNode nameNode = itemNode.SelectSingleNode("Name");
                XmlNode fullPathNode = itemNode.SelectSingleNode("FullPath");
                XmlNode type = itemNode.SelectSingleNode("Type");

                if (nameNode == null || fullPathNode == null || type == null) continue;

                // check whether project exists.
                if (File.Exists(Path.Combine(solutionPath, fullPathNode.InnerText)) == false) continue;

                // load project module
                var projectPath = Path.GetDirectoryName(fullPathNode.InnerText);
                var projectFileName = Path.GetFileName(fullPathNode.InnerText);

                ProjectLoader loader = new ProjectLoader();
                if (type.InnerText == new MiniCGenerator().Extension) loader = new MiniCLoader();

                result.Projects.Add(loader.LoadProject(Path.Combine(solutionPath, projectPath), projectFileName));
            }

            return result;
        }
    }
}
