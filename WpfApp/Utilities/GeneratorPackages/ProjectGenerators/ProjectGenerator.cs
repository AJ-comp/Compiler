using Parse.BackEnd.Target;
using WpfApp.Utilities.GeneratorPackages.ProjectStructs;

namespace WpfApp.Utilities.GeneratorPackages.ProjectGenerators
{
    public abstract class ProjectGenerator
    {
        public ProjectStruct ProjectStructure;
        
        public abstract string Extension { get; }


        public abstract ProjectStruct Generator(string projectPath, string projectName, Target target);
    }
}
