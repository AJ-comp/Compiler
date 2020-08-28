using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes
{
    public abstract class ArithmeticExprNode : BinaryExprNode
    {
        protected ArithmeticExprNode(AstSymbol node) : base(node)
        {
        }
    }
}
