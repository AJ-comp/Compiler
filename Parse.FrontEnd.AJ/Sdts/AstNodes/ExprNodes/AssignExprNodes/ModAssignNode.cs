using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.AssignExprNodes
{
    public class ModAssignNode : OperationAssignExprNode
    {
        public override IROperation Operation => IROperation.Mod;

        public ModAssignNode(AstSymbol node) : base(node)
        {
        }
    }
}
