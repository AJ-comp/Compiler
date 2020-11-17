using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.LogicalExprNodes
{
    public abstract class LogicalExprNode : BinaryExprNode
    {

        protected LogicalExprNode(AstSymbol node) : base(node)
        {
        }
    }
}
