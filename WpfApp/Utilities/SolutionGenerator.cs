using Parse.BackEnd.Target;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.MiniC;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using WpfApp.Models;
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

        public SolutionStruct Generate(string solutionPath, string solutionName, bool bCreateSolutionFolder, Grammar grammar, Target target)
        {
            SolutionStruct result = new SolutionStruct();

            if (bCreateSolutionFolder)
                solutionPath = Path.Combine(solutionPath, solutionName);

            result.Path = solutionPath;
            result.FullName = solutionName + ".ajn";
            result.Version = 1.0;

            if (grammar is MiniCGrammar)
                this.projectGenerator = new MiniCGenerator();

            var projectPath = solutionPath + "\\" + solutionName;
            result.Projects.Add(this.projectGenerator.Generator(projectPath, solutionName, target));

            return result;
        }
    }
}
