using Parse.BackEnd.Target;
using WpfApp.Utilities.GeneratorPackages.ProjectStructs;

namespace WpfApp.Utilities.GeneratorPackages.ProjectGenerators
{
    public abstract class ProjectGenerator
    {
        public ProjectStruct ProjectStructure;
        public enum Configure { Debug, Release }
        public abstract string Extension { get; }


        public abstract void Generator(string projectPath, string projectName, Target target);
    }
}
