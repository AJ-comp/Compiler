using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.LogicalExprNodes
{
    public class LessEqualNode : CompareExprNode
    {
        public override IRCompareSymbol CompareOper => IRCompareSymbol.LE;
        public LessEqualNode(AstSymbol node) : base(node)
        {
        }
    }
}
