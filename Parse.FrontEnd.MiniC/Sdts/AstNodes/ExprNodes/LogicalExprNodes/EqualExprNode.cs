using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.LogicalExprNodes
{
    public class EqualExprNode : CompareExprNode
    {
        public override IRCompareSymbol CompareOper => IRCompareSymbol.EQ;

        public EqualExprNode(AstSymbol node) : base(node)
        {
        }
    }
}
