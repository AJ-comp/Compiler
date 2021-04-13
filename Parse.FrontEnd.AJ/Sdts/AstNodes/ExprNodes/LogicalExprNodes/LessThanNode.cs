using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LogicalExprNodes
{
    public class LessThanNode : CompareExprNode
    {
        public override IRCompareSymbol CompareOper => IRCompareSymbol.LT;
        public LessThanNode(AstSymbol node) : base(node)
        {
        }
    }
}
