using System.IO;
using System.Xml;

namespace WpfApp.Utilities
{
    public class Generator
    {
        private XmlDocument xDoc = new XmlDocument();
        private string projectExtension = ".ajproj";
        private string solutionExtension = ".aj";

        public enum Configure { Debug, Release }

        private XmlNode GetDefaultPropertyNode(Configure conf)
        {
            XmlNode propertyGroup = xDoc.CreateElement("PropertyGroup");
            XmlNode configure = xDoc.CreateElement("Configuration");
            configure.InnerText = conf.ToString();
            propertyGroup.AppendChild(configure);

            XmlNode platform = xDoc.CreateElement("PlatForm");
            platform.InnerText = "Stm32";
            propertyGroup.AppendChild(platform);

            XmlNode optimize = xDoc.CreateElement("Optimize");
            optimize.InnerText = "false";
            propertyGroup.AppendChild(optimize);

            return propertyGroup;
        }


        private XmlNode GetProjectInfoNode(string projectPath, string projectName)
        {
            XmlNode project = xDoc.CreateElement("Project");
            XmlNode name = xDoc.CreateElement("Name");
            name.InnerText = projectName;
            project.AppendChild(name);

            XmlNode path = xDoc.CreateElement("FullPath");
            path.InnerText = Path.Combine(projectPath, projectName) + this.projectExtension;
            project.AppendChild(path);

            return project;
        }

        public void GenerateSolution(string solutionPath, string solutionName, bool bCreateSolutionFolder)
        {
            string projectPath = solutionPath;

            if(bCreateSolutionFolder)
            {
                solutionPath = Path.Combine(solutionPath, solutionName);
                Directory.CreateDirectory(solutionPath);
            }

            projectPath = solutionPath + "\\" + solutionName;
            this.GenerateProject(projectPath, solutionName);

            this.xDoc = new XmlDocument();

            XmlNode root = xDoc.CreateElement("Solution");
            XmlAttribute attr = xDoc.CreateAttribute("ToolVersion");
            attr.Value = "1.0";
            root.Attributes.Append(attr);

            // when create solution, soultion name is project name.
            var projectRelativePath = solutionName;
            root.AppendChild(this.GetProjectInfoNode(projectRelativePath, solutionName));

            xDoc.AppendChild(root);
            xDoc.Save(Path.Combine(solutionPath, solutionName) + this.solutionExtension);
        }

        public void GenerateProject(string projectPath, string projectName)
        {
            Directory.CreateDirectory(projectPath);

            this.xDoc = new XmlDocument();

            XmlNode root = xDoc.CreateElement("Project");
            XmlAttribute attr = xDoc.CreateAttribute("ToolVersion");
            attr.Value = "1.0";
            root.Attributes.Append(attr);

            root.AppendChild(this.GetDefaultPropertyNode(Configure.Debug));
            root.AppendChild(this.GetDefaultPropertyNode(Configure.Release));

            xDoc.AppendChild(root);
            xDoc.Save(Path.Combine(projectPath, projectName) + this.projectExtension);
        }
    }
}
