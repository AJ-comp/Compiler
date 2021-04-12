using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes
{
    public class PreIncExprNode : IncDecExprNode
    {
        public override Info ProcessInfo => Info.PreInc;

        public PreIncExprNode(AstSymbol node) : base(node, "++")
        {
        }
    }
}
