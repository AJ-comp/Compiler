using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.AssignExprNodes
{
    public class SubAssignNode : OperationAssignExprNode
    {
        public override IROperation Operation => IROperation.Sub;

        public SubAssignNode(AstSymbol node) : base(node)
        {
        }
    }
}
