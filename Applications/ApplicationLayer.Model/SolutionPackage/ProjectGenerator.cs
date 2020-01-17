using Parse.BackEnd.Target;

namespace ApplicationLayer.Models.SolutionPackage
{
    public abstract class ProjectGenerator
    {
        public ProjectStruct ProjectStructure;
        
        public abstract string Extension { get; }


        public abstract ProjectStruct CreateEmptyProject(string projectPath, string projectName, Target target, HirStruct parent);
        public abstract ProjectStruct CreateDefaultProject(string projectPath, string projectName, Target target, HirStruct parent);
    }


    public class LanguageExtensions
    {
        public static string MiniC { get; } = "mc";
        public static string AJ { get; } = "aj";
    }
}
