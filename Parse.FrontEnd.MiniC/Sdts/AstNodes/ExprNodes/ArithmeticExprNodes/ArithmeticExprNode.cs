using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes
{
    public abstract class ArithmeticExprNode : BinaryExprNode
    {
        protected ArithmeticExprNode(AstSymbol node) : base(node)
        {
        }
    }
}
