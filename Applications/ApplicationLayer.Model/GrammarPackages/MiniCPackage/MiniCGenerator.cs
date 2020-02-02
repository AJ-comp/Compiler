using ApplicationLayer.Models.SolutionPackage;
using Parse.BackEnd.Target;
using System.IO;
using static ApplicationLayer.Models.SolutionPackage.ProjectProperty;

namespace ApplicationLayer.Models.GrammarPackages.MiniCPackage
{
    public class MiniCGenerator : ProjectGenerator
    {
        public override string Extension { get; } = LanguageExtensions.MiniC;

        public override DefaultProjectStruct CreateDefaultProject(string projectPath, bool isAbsolutePath, string projectName, Target target, HirStruct parent)
        {
            DefaultProjectStruct result = this.CreateEmptyProject(projectPath, isAbsolutePath, projectName, target, parent);

            result.ReferenceFolder[0].Items.Add(new ReferenceFileStruct() { CurOPath = "MiniC", FullName = "System.dll" });
            result.ReferenceFolder[0].Items.Add(new ReferenceFileStruct() { CurOPath = "MiniC", FullName = "System.IO.dll" });
            result.ReferenceFolder[0].Items.Add(new ReferenceFileStruct() { CurOPath = "MiniC", FullName = "System.Data.dll" });

            string fileName = string.Format("main.{0}", this.Extension);
            string fileData = string.Empty;

            Directory.CreateDirectory(result.BaseOPath);
            File.WriteAllText(Path.Combine(result.BaseOPath, fileName), fileData);

            result.Items.Add(new DefaultFileStruct()
            {
                FullName = fileName,
                Data = fileData
            });

            result.SyncObjectToXml();

            return result;
        }

        public override DefaultProjectStruct CreateEmptyProject(string projectPath, bool isAbsolutePath, string projectName, Target target, HirStruct parent)
        {
            DefaultProjectStruct result = new DefaultProjectStruct
            {
                Parent = parent,

                CurOPath = projectPath,
                FullName = string.Format("{0}.{1}", projectName, this.Extension + "proj"),
                Version = 1.0
            };

            ProjectProperty debugProperty = new ProjectProperty();
            debugProperty.Mode = Configure.Debug;
            debugProperty.Target = target.Name;
            debugProperty.OptimizeLevel = 0;

            ProjectProperty releaseProperty = new ProjectProperty();
            releaseProperty.Mode = Configure.Release;
            releaseProperty.Target = target.Name;
            releaseProperty.OptimizeLevel = 0;

            result.Properties.Add(debugProperty);
            result.Properties.Add(releaseProperty);

            //            result.ReferenceFolder

            return result;
        }
    }
}
