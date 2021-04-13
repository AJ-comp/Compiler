using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes
{
    public class PostIncExprNode : IncDecExprNode
    {
        public override Info ProcessInfo => Info.PostInc;

        public PostIncExprNode(AstSymbol node) : base(node, "++")
        {
        }
    }
}
