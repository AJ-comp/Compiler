using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes
{
    public class PostDecExprNode : IncDecExprNode
    {
        public override Info ProcessInfo => Info.PostDec;

        public PostDecExprNode(AstSymbol node) : base(node, "--")
        {
        }
    }
}
