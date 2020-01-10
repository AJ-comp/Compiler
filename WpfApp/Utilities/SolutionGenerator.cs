using Parse.BackEnd.Target;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using WpfApp.Utilities.GeneratorPackages.ProjectGenerators;

namespace WpfApp.Utilities
{
    public class SolutionGenerator
    {
        private Dictionary<string, string> langExeDic = new Dictionary<string, string>();
        private XmlDocument xDoc = new XmlDocument();
        private ProjectGenerator projectGenerator;

        public string SolutionExtension { get; } = ".ajn";

        public SolutionGenerator()
        {
            this.langExeDic.Add(new MiniCGrammar().ToString(), "mc");
            this.langExeDic.Add(new AJGrammar().ToString(), "aj");
        }

        private XmlNode CreateProjectInfoNode(string projectPath, string projectName, string extension)
        {
            XmlNode project = xDoc.CreateElement("Project");
            XmlNode name = xDoc.CreateElement("Name");
            name.InnerText = projectName;
            project.AppendChild(name);

            XmlNode path = xDoc.CreateElement("FullPath");
            path.InnerText = string.Format("{0}.{1}", Path.Combine(projectPath, projectName), extension + "proj");
            project.AppendChild(path);

            return project;
        }

        public void GenerateSolution(string solutionPath, string solutionName, bool bCreateSolutionFolder, Grammar grammar, Target target)
        {
            if (grammar is MiniCGrammar)
                this.projectGenerator = new MiniCGenerator();

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
            root.AppendChild(this.CreateProjectInfoNode(projectRelativePath, solutionName, this.projectGenerator.Extension));

            xDoc.AppendChild(root);
            xDoc.Save(Path.Combine(solutionPath, solutionName) + this.SolutionExtension);
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
            if (grammar is MiniCGrammar)
                this.projectGenerator = new MiniCGenerator();

            this.projectGenerator.Generator(projectPath, projectName, target);
        }
    }
}
