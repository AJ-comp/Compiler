using System.IO;
using System.Xml;
using WpfApp.Models;

namespace WpfApp.Utilities
{
    public class Loader
    {
        public ProjectStruct LoadProject(string projectPath, string projectName)
        {
            ProjectStruct result = new ProjectStruct() { Name = projectName };



            return result;
        }

        public SolutionStruct LoadSolution(string solutionPath, string solutionName)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Path.Combine(solutionPath, solutionName));

            XmlNodeList itemNodes = xmlDoc.SelectNodes("//Solution/Project");

            SolutionStruct result = new SolutionStruct() { Name = solutionName };

            foreach (XmlNode itemNode in itemNodes)
            {
                XmlNode nameNode = itemNode.SelectSingleNode("Name");
                XmlNode fullPathNode = itemNode.SelectSingleNode("FullPath");

                if (nameNode == null || fullPathNode == null) continue;

                // check whether project exists.
                if (File.Exists(Path.Combine(solutionPath, fullPathNode.InnerText)) == false) continue;

                // load project module
                ProjectStruct projectStruct = this.LoadProject(fullPathNode.InnerText, nameNode.InnerText);


                result.Projects.Add(new ProjectStruct() { Name = nameNode.InnerText });
            }

            return result;
        }
    }
}
