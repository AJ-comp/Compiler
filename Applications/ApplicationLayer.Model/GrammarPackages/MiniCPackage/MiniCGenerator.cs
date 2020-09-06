using ApplicationLayer.Common;
using ApplicationLayer.Common.Utilities;
using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.Models.SolutionPackage.MiniCPackage;
using Parse.BackEnd.Target;
using System.IO;
using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.Models.GrammarPackages.MiniCPackage
{
    public class MiniCGenerator : ProjectGenerator
    {
        public override string Extension { get; } = LanguageExtensions.MiniCSource;

        public override ProjectTreeNodeModel CreateEmptyProject(string solutionPath, string projectPath, string projectName, Target target)
        {
            MiniCProjectTreeNodeModel result = new MiniCProjectTreeNodeModel(projectPath, projectName, target)
            {
                OuterDependencies = new FilterTreeNodeModel(CommonResource.ExternDependency)
            };

            var resourceFilter = new FilterTreeNodeModel(CommonResource.ResourceFiles);
            result.AddFilter(resourceFilter);

            var headerFilter = new FilterTreeNodeModel(CommonResource.HeaderFiles);
            result.AddFilter(headerFilter);

            var sourceFilter = new FilterTreeNodeModel(CommonResource.SourceFiles);
            result.AddFilter(sourceFilter);

            return result;
        }

        public override ProjectTreeNodeModel CreateDefaultProject(string solutionPath, string projectPath, string projectName, Target target)
        {
            MiniCProjectTreeNodeModel result = new MiniCProjectTreeNodeModel(projectPath, projectName, target)
            {
                OuterDependencies = new FilterTreeNodeModel(CommonResource.ExternDependency)
            };

            var resourceFilter = new FilterTreeNodeModel(CommonResource.ResourceFiles);
            result.AddFilter(resourceFilter);

            var headerFilter = new FilterTreeNodeModel(CommonResource.HeaderFiles);
            result.AddFilter(headerFilter);

            string path = System.IO.Path.Combine(solutionPath, projectPath);
            string fileName = string.Format("main.{0}", this.Extension);
            string defaultContent = "void main()\r\n{\r\n}";

            Directory.CreateDirectory(path);
            fileName = FileExtend.CreateFile(path, fileName, defaultContent);

            var sourceFilter = new FilterTreeNodeModel(CommonResource.SourceFiles);
            var sourceFile = new SourceFileTreeNodeModel(string.Empty, fileName);
            sourceFilter.AddFile(sourceFile);
            result.AddFilter(sourceFilter);

            result.SyncWithCurrentValue();

            return result;
        }
    }
}
