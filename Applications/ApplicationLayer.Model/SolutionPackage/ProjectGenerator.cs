using Parse.BackEnd.Target;

namespace ApplicationLayer.Models.SolutionPackage
{
    public abstract class ProjectGenerator
    {
        public ProjectStruct ProjectStructure;
        
        public abstract string Extension { get; }


        public abstract ProjectStruct Generator(string projectPath, string projectName, Target target);
    }
}
