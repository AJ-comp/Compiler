using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.AssignExprNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.MiddleEnd.IR;
using Parse.MiddleEnd.IR.LLVM;
using Parse.MiddleEnd.IR.LLVM.Expressions.AssignExpressions;
using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions;
using Parse.Types;

namespace Parse.FrontEnd.IRGenerator
{
    public partial class IRExpressionGenerator
    {
        private static IRExpression AssignNodeToIRExpression(SdtsNode node, object param)
        {
            var cNode = node as AssignNode;
            var ssaTable = param as LLVMSSATable;

            return new LLVMAssignExpression(cNode.LeftVar, 
                                                                cNode.Right.ExecuteToIRExpression(ssaTable) as LLVMExprExpression,
                                                                ssaTable);
        }

        private static IRExpression ArithmeticCommonLogic(SdtsNode node, object param, IROperation operation)
        {
            var cNode = node as AddExprNode;
            var ssaTable = param as LLVMSSATable;

            var leftIR = cNode.Left.ExecuteToIRExpression(ssaTable) as LLVMExprExpression;
            var rightIR = cNode.Right.ExecuteToIRExpression(ssaTable) as LLVMExprExpression;

            return new LLVMArithmeticExpression(leftIR, rightIR, operation, ssaTable);
        }

        private static IRExpression AddExprNodeToIRExpression(SdtsNode node, object param)
            => ArithmeticCommonLogic(node, param, IROperation.Add);

        private static IRExpression SubExprNodeToIRExpression(SdtsNode node, object param)
            => ArithmeticCommonLogic(node, param, IROperation.Sub);

        private static IRExpression MulExprNodeToIRExpression(SdtsNode node, object param)
            => ArithmeticCommonLogic(node, param, IROperation.Mul);

        private static IRExpression DivExprNodeToIRExpression(SdtsNode node, object param)
            => ArithmeticCommonLogic(node, param, IROperation.Div);

        private static IRExpression ModExprNodeToIRExpression(SdtsNode node, object param)
            => ArithmeticCommonLogic(node, param, IROperation.Mod);

        private static IRExpression UseIdentNodeToIRExpression(SdtsNode sdtsNode, object param)
            => new LLVMUseVarExpression((sdtsNode as UseIdentNode).VarData, param as LLVMSSATable);


        private static IRExpression LiteralNodeToIRExpression(SdtsNode sdtsNode, object param)
        {
            var ssaTable = param as LLVMSSATable;

            if (sdtsNode is IntLiteralNode)
            {
                var node = sdtsNode as IntLiteralNode;

                return new LLVMConstantExpression(node.Result, ssaTable);
            }

            return null;
        }
    }
}
