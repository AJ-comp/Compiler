using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LogicalExprNodes
{
    public class NotEqualExprNode : CompareExprNode
    {
        public override IRCompareSymbol CompareOper => IRCompareSymbol.NE;
        public NotEqualExprNode(AstSymbol node) : base(node)
        {
        }
    }
}
