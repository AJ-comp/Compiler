using Parse.BackEnd.Target;
using static ApplicationLayer.Models.SolutionPackage.ProjectProperty;

namespace ApplicationLayer.Models.SolutionPackage
{
    public class MiniCGenerator : ProjectGenerator
    {
        public override string Extension { get; } = "mc";

        public override ProjectStruct CreateDefaultProject(string projectPath, string projectName, Target target, HirStruct parent)
        {
            ProjectStruct result = this.CreateEmptyProject(projectPath, projectName, target, parent);

            result.ReferenceFolder[0].Items.Add(new ReferenceFileStruct() { OPath = "MiniC", FullName = "System.dll" });
            result.ReferenceFolder[0].Items.Add(new ReferenceFileStruct() { OPath = "MiniC", FullName = "System.IO.dll" });
            result.ReferenceFolder[0].Items.Add(new ReferenceFileStruct() { OPath = "MiniC", FullName = "System.Data.dll" });

            result.Items.Add(new FileStruct()
            {
                FullName = string.Format("main.{0}", this.Extension)
            });

            return result;
        }

        public override ProjectStruct CreateEmptyProject(string projectPath, string projectName, Target target, HirStruct parent)
        {
            ProjectStruct result = new ProjectStruct
            {
                Parent = parent,

                OPath = projectPath,
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
