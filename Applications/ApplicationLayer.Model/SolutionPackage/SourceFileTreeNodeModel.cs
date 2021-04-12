using Parse.FrontEnd;
using Parse.FrontEnd.MiniC.Sdts.Expressions;

namespace ApplicationLayer.Models.SolutionPackage
{
    public class SourceFileTreeNodeModel : FileTreeNodeModel
    {
        public SdtsNode Ast { get; set; }
        public FinalExpression FinalExpression { get; set; }

        public SourceFileTreeNodeModel(string path, string filename) : base(path, filename)
        {
        }
    }
}
