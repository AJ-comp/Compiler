using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.LogicalExprNodes
{
    public class GreaterThanNode : CompareExprNode
    {
        public override IRCompareSymbol CompareOper => IRCompareSymbol.GT;

        public GreaterThanNode(AstSymbol node) : base(node)
        {
        }
    }
}
