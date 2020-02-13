using ApplicationLayer.Models.SolutionPackage;
using Parse.BackEnd.Target;
using System.IO;
using static ApplicationLayer.Models.SolutionPackage.ProjectProperty;

namespace ApplicationLayer.Models.GrammarPackages.MiniCPackage
{
    public class MiniCGenerator : ProjectGenerator
    {
        public override string Extension { get; } = LanguageExtensions.MiniC;

        public override DefaultProjectHier CreateDefaultProject(string projectPath, bool isAbsolutePath, string projectName, Target target, HierarchicalData parent)
        {
            DefaultProjectHier result = this.CreateEmptyProject(projectPath, isAbsolutePath, projectName, target, parent);

            result.ReferenceFolder[0].Items.Add(new ReferenceFileStruct("MiniC", "System.dll"));
            result.ReferenceFolder[0].Items.Add(new ReferenceFileStruct("MiniC", "System.IO.dll"));
            result.ReferenceFolder[0].Items.Add(new ReferenceFileStruct("MiniC", "System.Data.dll"));

            string fileName = string.Format("main.{0}", this.Extension);
            string fileData = "void main()\r\n{\r\n}";

            var fileHier = new DefaultFileHier(fileName)
            {
                Data = fileData
            };
            result.Items.Add(fileHier);
            fileHier.Save();

            result.Commit();

            return result;
        }

        public override DefaultProjectHier CreateEmptyProject(string projectPath, bool isAbsolutePath, string projectName, Target target, HierarchicalData parent)
        {
            DefaultProjectHier result = new DefaultProjectHier(projectPath, string.Format("{0}.{1}", projectName, this.Extension + "proj"))
            {
                Parent = parent,
                CurrentVersion = 1.0
            };

            ProjectProperty debugProperty = new ProjectProperty
            {
                Mode = Configure.Debug,
                Target = target.Name,
                OptimizeLevel = 0
            };

            ProjectProperty releaseProperty = new ProjectProperty
            {
                Mode = Configure.Release,
                Target = target.Name,
                OptimizeLevel = 0
            };

            result.CurrentProperties.Add(debugProperty);
            result.CurrentProperties.Add(releaseProperty);

            //            result.ReferenceFolder

            return result;
        }
    }
}
