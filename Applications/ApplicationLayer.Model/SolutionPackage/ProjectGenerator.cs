using ApplicationLayer.Models.GrammarPackages.MiniCPackage;
using Parse.BackEnd.Target;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.MiniC;

namespace ApplicationLayer.Models.SolutionPackage
{
    public abstract class ProjectGenerator
    {        
        public abstract string Extension { get; }

        public abstract ProjectTreeNodeModel CreateEmptyProject(string solutionPath,
                                                                                             ProjectData projectData,
                                                                                             Target target);
        public abstract ProjectTreeNodeModel CreateDefaultProject(string solutionPath, 
                                                                                              ProjectData projectData,
                                                                                              Target target);

        public static ProjectGenerator CreateProjectGenerator(Grammar grammar)
        {
            ProjectGenerator result = null;
            if (grammar is MiniCGrammar) result = new MiniCGenerator();

            return result;
        }
    }
}
