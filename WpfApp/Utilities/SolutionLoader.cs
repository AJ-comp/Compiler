using System.IO;
using System.Xml;
using WpfApp.Models;
using WpfApp.Utilities.GeneratorPackages.ProjectStructs;

namespace WpfApp.Utilities
{
    public class SolutionLoader
    {
        public ProjectStruct LoadProject(string projectPath, string projectFileName)
        {
            ProjectStruct result = new ProjectStruct() { Name = projectFileName };

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Path.Combine(projectPath, projectFileName));

            XmlNodeList propertyNodes = xmlDoc.SelectNodes("//Project/PropertyGroup");
            foreach(XmlNode propertyNode in propertyNodes)
            {
                XmlNode configureNode = propertyNode.SelectSingleNode("Configuration");
                XmlNode fullPathNode = propertyNode.SelectSingleNode("PlatForm");
                XmlNode optimizeNode = propertyNode.SelectSingleNode("Optimize");
            }

            XmlNodeList refGroupNodes = xmlDoc.SelectNodes("//Project/ReferenceGroup");
            foreach(XmlNode refNode in refGroupNodes)
            {
                XmlNodeList itemNodes = refNode.SelectNodes("Item");

            }

            XmlNodeList itemGroupNodes = xmlDoc.SelectNodes("//Project/ItemGroup");
            foreach (XmlNode itemNode in itemGroupNodes)
            {
                XmlNodeList itemNodes = itemNode.SelectNodes("Item");

                foreach(XmlNode item in itemNodes)
                {
                    result.Items.Add(new FileStruct() { Name = item.InnerText });
                }
            }


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
                var projectPath = Path.GetDirectoryName(fullPathNode.InnerText);
                var projectFileName = Path.GetFileName(fullPathNode.InnerText);
                ProjectStruct projectStruct = this.LoadProject(Path.Combine(solutionPath, projectPath), projectFileName);


                result.Projects.Add(new ProjectStruct() { Name = nameNode.InnerText });
            }

            return result;
        }
    }
}
