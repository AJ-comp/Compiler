using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.AssignExprNodes
{
    public class MulAssignNode : OperationAssignExprNode
    {
        public override IROperation Operation => IROperation.Mul;

        public MulAssignNode(AstSymbol node) : base(node)
        {
        }
    }
}
