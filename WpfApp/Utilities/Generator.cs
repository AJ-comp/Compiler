using Parse.BackEnd.Target;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Xml;

namespace WpfApp.Utilities
{
    public class Generator
    {
        private Dictionary<string, string> langExeDic = new Dictionary<string, string>();
        private XmlDocument xDoc = new XmlDocument();
        private string solutionExtension = ".ajn";

        public enum Configure { Debug, Release }


        public Generator()
        {
            this.langExeDic.Add(new MiniCGrammar().ToString(), "mc");
            this.langExeDic.Add(new AJGrammar().ToString(), "aj");
        }

        private XmlNode CreateDefaultPropertyNode(Configure conf, Target target)
        {
            XmlNode propertyGroup = xDoc.CreateElement("PropertyGroup");
            XmlNode configure = xDoc.CreateElement("Configuration");
            configure.InnerText = conf.ToString();
            propertyGroup.AppendChild(configure);

            XmlNode platform = xDoc.CreateElement("PlatForm");
            platform.InnerText = target.Name;
            propertyGroup.AppendChild(platform);

            XmlNode optimize = xDoc.CreateElement("Optimize");
            optimize.InnerText = "false";
            propertyGroup.AppendChild(optimize);

            return propertyGroup;
        }

        private XmlNode CreateReferenceNode(StringCollection items)
        {
            XmlNode referGroup = xDoc.CreateElement("ReferenceGroup");

            foreach(var item in items)
            {
                XmlNode referItem = xDoc.CreateElement("Item");
                referItem.InnerText = item;
                referGroup.AppendChild(referItem);
            }

            return referGroup;
        }

        private XmlNode CreateItemNode(string item)
        {
            XmlNode itemGroup = xDoc.CreateElement("ItemGroup");

            XmlNode itemNode = xDoc.CreateElement("Item");
            itemNode.InnerText = item;
            itemGroup.AppendChild(itemNode);

            return itemGroup;
        }


        private XmlNode CreateProjectInfoNode(string projectPath, string projectName, Grammar grammar)
        {
            XmlNode project = xDoc.CreateElement("Project");
            XmlNode name = xDoc.CreateElement("Name");
            name.InnerText = projectName;
            project.AppendChild(name);

            XmlNode path = xDoc.CreateElement("FullPath");
            path.InnerText = Path.Combine(projectPath, projectName) + this.langExeDic[grammar.ToString()] + "proj";
            project.AppendChild(path);

            return project;
        }

        private void GenerateMiniCProject(string projectPath, string projectName, string extension, Target target)
        {
            Directory.CreateDirectory(projectPath);

            this.xDoc = new XmlDocument();

            XmlNode root = xDoc.CreateElement("Project");
            XmlAttribute attr = xDoc.CreateAttribute("ToolVersion");
            attr.Value = "1.0";
            root.Attributes.Append(attr);

            root.AppendChild(this.CreateDefaultPropertyNode(Configure.Debug, target));
            root.AppendChild(this.CreateDefaultPropertyNode(Configure.Release, target));
            root.AppendChild(this.CreateReferenceNode(new StringCollection() { "System", "System.Data", "System.Collection" }));
            root.AppendChild(this.CreateItemNode("Class." + extension));

            xDoc.AppendChild(root);
            xDoc.Save(Path.Combine(projectPath, projectName) + extension + "proj");
        }

        public void GenerateSolution(string solutionPath, string solutionName, bool bCreateSolutionFolder, Grammar grammar, Target target)
        {
            string projectPath = solutionPath;

            if(bCreateSolutionFolder)
            {
                solutionPath = Path.Combine(solutionPath, solutionName);
                Directory.CreateDirectory(solutionPath);
            }

            projectPath = solutionPath + "\\" + solutionName;
            this.GenerateProject(projectPath, solutionName, grammar, target);

            this.xDoc = new XmlDocument();
            
            XmlNode root = xDoc.CreateElement("Solution");
            XmlAttribute attr = xDoc.CreateAttribute("ToolVersion");
            attr.Value = "1.0";
            root.Attributes.Append(attr);

            // when create solution, soultion name is project name.
            var projectRelativePath = solutionName;
            root.AppendChild(this.CreateProjectInfoNode(projectRelativePath, solutionName, grammar));

            xDoc.AppendChild(root);
            xDoc.Save(Path.Combine(solutionPath, solutionName) + this.solutionExtension);
        }

        /// <summary>
        /// This function is a gateway to generate a project.
        /// </summary>
        /// <param name="projectPath">The project path</param>
        /// <param name="projectName">The project name</param>
        /// <param name="grammar">The grmmar to use in the project</param>
        /// <param name="target">The target to use in the project</param>
        public void GenerateProject(string projectPath, string projectName, Grammar grammar, Target target)
        {
            if (grammar.ToString() == new MiniCGrammar().ToString())
                this.GenerateMiniCProject(projectPath, projectName, this.langExeDic[grammar.ToString()], target);
        }
    }
}
