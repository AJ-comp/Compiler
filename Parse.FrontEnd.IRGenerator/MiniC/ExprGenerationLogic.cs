using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.AssignExprNodes;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.MiddleEnd.IR;
using Parse.MiddleEnd.IR.LLVM;
using Parse.MiddleEnd.IR.LLVM.Expressions.AssignExpressions;
using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions;

namespace Parse.FrontEnd.IRGenerator
{
    public partial class IRExpressionGenerator
    {
        private static IRExpression AssignNodeToIRExpression(SdtsNode node, object param)
        {
            var cNode = node as AssignNode;
            var ssaTable = param as LLVMSSATable;

            return new LLVMAssignExpression(cNode.LeftNode.ExecuteToIRExpression(ssaTable) as LLVMExprExpression, 
                                                                cNode.RightNode.ExecuteToIRExpression(ssaTable) as LLVMExprExpression,
                                                                ssaTable);
        }
    }
}
