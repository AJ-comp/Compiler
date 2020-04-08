using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.Models.SolutionPackage.MiniCPackage;
using Parse.BackEnd.Target;

using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.Models.GrammarPackages.MiniCPackage
{
    public class MiniCGenerator : ProjectGenerator
    {
        public override string Extension { get; } = LanguageExtensions.MiniC;

        public override ProjectTreeNodeModel CreateEmptyProject(string projectPath, bool isAbsolutePath, string projectName, Target target)
        {
            MiniCProjectTreeNodeModel result = new MiniCProjectTreeNodeModel(projectPath, projectName, target);

            result.OuterDependencies = new FilterTreeNodeModel(CommonResource.ExternDependency);

            var resourceFilter = new FilterTreeNodeModel(CommonResource.ResourceFiles);
            result.AddFilter(resourceFilter);

            var headerFilter = new FilterTreeNodeModel(CommonResource.HeaderFiles);
            result.AddFilter(headerFilter);

            var sourceFilter = new FilterTreeNodeModel(CommonResource.SourceFiles);
            result.AddFilter(sourceFilter);

            return result;
        }

        public override ProjectTreeNodeModel CreateDefaultProject(string projectPath, bool isAbsolutePath, string projectName, Target target)
        {
            MiniCProjectTreeNodeModel result = new MiniCProjectTreeNodeModel(projectPath, projectName, target);

            result.OuterDependencies = new FilterTreeNodeModel(CommonResource.ExternDependency);

            var resourceFilter = new FilterTreeNodeModel(CommonResource.ResourceFiles);
            result.AddFilter(resourceFilter);

            var headerFilter = new FilterTreeNodeModel(CommonResource.HeaderFiles);
            result.AddFilter(headerFilter);

            var sourceFilter = new FilterTreeNodeModel(CommonResource.SourceFiles);
            var sourceFile = new MiniCFileTreeNodeModel(projectPath, string.Format("main.{0}", this.Extension));
            sourceFilter.AddFile(sourceFile);
            result.AddFilter(sourceFilter);

            result.SyncWithCurrentValue();

            return result;
        }
    }
}
