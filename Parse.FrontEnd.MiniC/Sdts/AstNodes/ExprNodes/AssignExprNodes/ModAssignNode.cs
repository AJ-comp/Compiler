using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.AssignExprNodes
{
    public class ModAssignNode : OperationAssignExprNode
    {
        public override IROperation Operation => IROperation.Mod;

        public ModAssignNode(AstSymbol node) : base(node)
        {
        }
    }
}
