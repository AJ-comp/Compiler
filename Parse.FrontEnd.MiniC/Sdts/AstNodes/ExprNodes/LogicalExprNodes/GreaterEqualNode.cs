using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.LogicalExprNodes
{
    public class GreaterEqualNode : CompareExprNode
    {
        public override IRCompareSymbol CompareOper => IRCompareSymbol.GE;

        public GreaterEqualNode(AstSymbol node) : base(node)
        {
        }
    }
}
