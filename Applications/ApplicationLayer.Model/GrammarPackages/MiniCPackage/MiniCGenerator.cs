using ApplicationLayer.Common;
using ApplicationLayer.Common.Utilities;
using ApplicationLayer.Models.SolutionPackage;
using ApplicationLayer.Models.SolutionPackage.MiniCPackage;
using Parse.BackEnd.Target;
using System;
using System.IO;
using CommonResource = ApplicationLayer.Define.Properties.Resources;

namespace ApplicationLayer.Models.GrammarPackages.MiniCPackage
{
    public class MiniCGenerator : ProjectGenerator
    {
        public override string Extension { get; } = LanguageExtensions.MiniCSource;

        public override ProjectTreeNodeModel CreateEmptyProject(string solutionPath, 
                                                                                             ProjectData projectData,
                                                                                             Target target)
        {
            MiniCProjectTreeNodeModel result = new MiniCProjectTreeNodeModel(projectData, target)
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

        public override ProjectTreeNodeModel CreateDefaultProject(string solutionPath, 
                                                                                               ProjectData projectData,
                                                                                               Target target)
        {
            MiniCProjectTreeNodeModel result = new MiniCProjectTreeNodeModel(projectData, target)
            {
                OuterDependencies = new FilterTreeNodeModel(CommonResource.ExternDependency)
            };

            var resourceFilter = new FilterTreeNodeModel(CommonResource.ResourceFiles);
            result.AddFilter(resourceFilter);

            var headerFilter = new FilterTreeNodeModel(CommonResource.HeaderFiles);
            result.AddFilter(headerFilter);

            string projAbsPath = Path.Combine(solutionPath, projectData.ProjectName);
            string fileName = string.Format("main.{0}", Extension);

            string defaultContent = string.Format("namespace {0}" + Environment.NewLine, projectData.ProjectName);
            defaultContent += "{" + Environment.NewLine;
            defaultContent += GetDefaultFunction("main");
            defaultContent += "}" + Environment.NewLine;

            Directory.CreateDirectory(projAbsPath);
            fileName = FileExtend.CreateFile(projAbsPath, fileName, defaultContent);

            var sourceFilter = new FilterTreeNodeModel(CommonResource.SourceFiles);
            var sourceFile = new SourceFileTreeNodeModel(string.Empty, fileName);
            sourceFilter.AddFile(sourceFile);
            result.AddFilter(sourceFilter);

            result.SyncWithCurrentValue();

            return result;
        }


        private string GetDefaultFunction(string functionName)
        {
            string result = string.Format("    void {0}()" + Environment.NewLine, functionName);
            result += "    {" + Environment.NewLine;
            result += "    }" + Environment.NewLine;

            return result;
        }
    }
}
