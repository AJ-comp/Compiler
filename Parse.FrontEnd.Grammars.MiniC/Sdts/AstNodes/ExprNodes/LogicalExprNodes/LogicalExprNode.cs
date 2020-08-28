using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.LogicalExprNodes
{
    public abstract class LogicalExprNode : BinaryExprNode
    {

        protected LogicalExprNode(AstSymbol node) : base(node)
        {
        }
    }
}
