using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes
{
    public class PreDecExprNode : IncDecExprNode
    {
        public override Info ProcessInfo => Info.PreDec;

        public PreDecExprNode(AstSymbol node) : base(node, "--")
        {
        }
    }
}
