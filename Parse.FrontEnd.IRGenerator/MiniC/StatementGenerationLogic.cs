using Parse.FrontEnd.MiniC.Sdts.AstNodes.StatementNodes;
using Parse.MiddleEnd.IR;
using Parse.MiddleEnd.IR.LLVM;
using Parse.MiddleEnd.IR.LLVM.Expressions;

namespace Parse.FrontEnd.IRGenerator
{
    public partial class IRExpressionGenerator
    {
        /*
        private static IRExpression CompoundStNodeToIRExpression(SdtsNode node, object param)
        {
            CompoundStNode cNode = node as CompoundStNode;
            var ssaTable = param as LLVMSSATable;
            LLVMBlockExpression blockExpression = new LLVMBlockExpression(ssaTable);

            blockExpression.AddItem(new LLVMLocalVarListExpression(cNode.LocalVars, ssaTable));

            // statment list (if, while, call ...)
            if (cNode.StatListNode != null)
            {
                foreach (var statement in cNode.StatListNode.StatementNodes)
                    blockExpression.AddItem(statement.ExecuteToIRExpression(ssaTable) as LLVMDependencyExpression);
            }

            return blockExpression;
        }
        */
    }
}
