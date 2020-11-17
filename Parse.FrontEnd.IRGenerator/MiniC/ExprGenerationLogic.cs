using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.AssignExprNodes;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.LogicalExprNodes;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes;
using Parse.MiddleEnd.IR;
using Parse.MiddleEnd.IR.LLVM;
using Parse.MiddleEnd.IR.LLVM.Expressions.AssignExpressions;
using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions;
using System.Collections.Generic;

namespace Parse.FrontEnd.IRGenerator
{
    public partial class IRExpressionGenerator
    {
        private static IRExpression CallNodeToIRExpression(SdtsNode node, object param)
        {
            var cNode = node as CallNode;
            var ssaTable = param as LLVMSSATable;
            List<LLVMExprExpression> paramList = new List<LLVMExprExpression>();

            foreach (var p in cNode.Params)
                paramList.Add(p.ExecuteToIRExpression(ssaTable) as LLVMExprExpression);

            return new LLVMCallExpression(cNode.FuncData.ToIRFuncData(), paramList, ssaTable);
        }

        private static IRExpression AssignNodeToIRExpression(SdtsNode node, object param)
        {
            var cNode = node as AssignNode;
            var ssaTable = param as LLVMSSATable;

            return new LLVMAssignExpression(cNode.Left.ExecuteToIRExpression(ssaTable) as LLVMExprExpression, 
                                                                cNode.Right.ExecuteToIRExpression(ssaTable) as LLVMExprExpression,
                                                                ssaTable);
        }

        private static IRExpression ArithmeticCommonLogic(SdtsNode node, object param, IROperation operation)
        {
            var cNode = node as BinaryExprNode;
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



        private static IRExpression CompareCommonLogic(SdtsNode node, object param, IRCondition condition)
        {
            var cNode = node as LogicalExprNode;
            var ssaTable = param as LLVMSSATable;

            var leftIR = cNode.Left.ExecuteToIRExpression(ssaTable) as LLVMExprExpression;
            var rightIR = cNode.Right.ExecuteToIRExpression(ssaTable) as LLVMExprExpression;

            return new LLVMCompareOpExpression(leftIR, rightIR, condition, ssaTable);
        }

        private static IRExpression EqualExprNodeToIRExpression(SdtsNode node, object param)
            => CompareCommonLogic(node, param, IRCondition.EQ);

        private static IRExpression NotEqualExprNodeToIRExpression(SdtsNode node, object param)
            => CompareCommonLogic(node, param, IRCondition.NE);

        private static IRExpression GreaterThanExprNodeToIRExpression(SdtsNode node, object param)
            => CompareCommonLogic(node, param, IRCondition.GT);

        private static IRExpression GreaterEqualExprNodeToIRExpression(SdtsNode node, object param)
            => CompareCommonLogic(node, param, IRCondition.GE);

        private static IRExpression LessThanExprNodeToIRExpression(SdtsNode node, object param)
            => CompareCommonLogic(node, param, IRCondition.LT);

        private static IRExpression LessEqualExprNodeToIRExpression(SdtsNode node, object param)
            => CompareCommonLogic(node, param, IRCondition.LE);

        private static IRExpression PreIncExprNodeToIRExpression(SdtsNode node, object param)
        {
            return new LLVMIncDecExpression((node as IncDecExprNode).UseIdentNode.VarData,
                                                                LLVMIncDecExpression.Info.PreInc,
                                                                param as LLVMSSATable);
        }

        private static IRExpression PreDecExprNodeToIRExpression(SdtsNode node, object param)
        {
            return new LLVMIncDecExpression((node as IncDecExprNode).UseIdentNode.VarData,
                                                                LLVMIncDecExpression.Info.PreDec,
                                                                param as LLVMSSATable);
        }

        private static IRExpression PostIncExprNodeToIRExpression(SdtsNode node, object param)
        {
            return new LLVMIncDecExpression((node as IncDecExprNode).UseIdentNode.VarData,
                                                                LLVMIncDecExpression.Info.PostInc,
                                                                param as LLVMSSATable);
        }

        private static IRExpression PostDecExprNodeToIRExpression(SdtsNode node, object param)
        {
            return new LLVMIncDecExpression((node as IncDecExprNode).UseIdentNode.VarData,
                                                                LLVMIncDecExpression.Info.PostDec,
                                                                param as LLVMSSATable);
        }

        private static IRExpression DeRefExprNodeToIRExpression(SdtsNode node, object param)
        {
            var cNode = node as DeRefExprNode;
            var ssaTable = param as LLVMSSATable;

            return new LLVMDeRefExpression(cNode.IdentNode.ExecuteToIRExpression(ssaTable) as LLVMUseVarExpression,
                                                                param as LLVMSSATable);
        }

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
