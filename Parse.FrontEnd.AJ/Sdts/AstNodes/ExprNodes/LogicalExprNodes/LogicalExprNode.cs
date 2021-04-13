using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LogicalExprNodes
{
    public abstract class LogicalExprNode : BinaryExprNode
    {
        protected LogicalExprNode(AstSymbol node) : base(node)
        {
        }
    }
}
