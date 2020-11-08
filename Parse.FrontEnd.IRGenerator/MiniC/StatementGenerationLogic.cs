using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables;
using Parse.MiddleEnd.IR;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.LLVM;
using Parse.MiddleEnd.IR.LLVM.Expressions;
using Parse.MiddleEnd.IR.LLVM.Expressions.AssignExpressions;
using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions;
using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions.LogicalExpressions;
using Parse.MiddleEnd.IR.LLVM.Expressions.StmtExpressions;
using Parse.MiddleEnd.IR.LLVM.Models;
using Parse.Types.ConstantTypes;
using System.Collections.Generic;

namespace Parse.FrontEnd.IRGenerator
{
    public partial class IRExpressionGenerator
    {
        private static IRExpression CommonCondStatementToIRExpression(SdtsNode node, object param)
        {
            var cNode = node as CondStatementNode;
            var ssaTable = param as LLVMSSATable;

            var irExpression = cNode.Condition.ExecuteToIRExpression(ssaTable);

            // if condition statement is not logical statement, it has to convert to logical statement.
            // (ex : (a + b)  convert to  (a + b) != 0)
            LLVMLogicalOpExpression logicalOp = null;
            if (irExpression is LLVMLogicalOpExpression)
            {
                logicalOp = irExpression as LLVMLogicalOpExpression;
            }
            else
            {
                logicalOp = new LLVMCompareOpExpression(irExpression as LLVMExprExpression,
                                                                                    new LLVMConstantExpression(new IntConstant(0), ssaTable), 
                                                                                    IRCondition.NE, 
                                                                                    ssaTable);
            }

            // process by case 
            var trueStatement = cNode.TrueStatement.ExecuteToIRExpression(ssaTable) as LLVMStmtExpression;
            if (cNode is IfElseStatementNode)
            {
                var ifElseNode = cNode as IfElseStatementNode;

                var falseStatement = ifElseNode.FalseStatement.ExecuteToIRExpression(ssaTable) as LLVMStmtExpression;
                return new LLVMIFExpression(logicalOp, trueStatement, falseStatement, ssaTable);
            }
            else if (cNode is IfStatementNode) return new LLVMIFExpression(logicalOp, trueStatement, null, ssaTable);
            else if (cNode is WhileStatementNode) return new LLVMWhileExpression(logicalOp, trueStatement, ssaTable);

            return null;
        }

        private static IRExpression CompoundStNodeToIRExpression(SdtsNode node, object param)
        {
            CompoundStNode cNode = node as CompoundStNode;
            var ssaTable = param as LLVMSSATable;
            LLVMBlockExpression blockExpression = new LLVMBlockExpression(ssaTable);

            // param variable declaration
            List<IRVar> paramVars = new List<IRVar>();
            foreach (var varRecord in cNode.SymbolTable.VarTable)
            {
                if (varRecord.DefineField.VariableProperty == VarProperty.Param) paramVars.Add(varRecord.DefineField);
            }

            blockExpression.AddItem(new LLVMParamListAndReturnExpression(paramVars, null, ssaTable));

            // local variable declaration
            List<CalculationInfo> declareInfos = new List<CalculationInfo>();
            foreach (var varRecord in cNode.SymbolTable.VarTable)
            {
                if (varRecord.DefineField.VariableProperty == VarProperty.Param) continue;

                var initExpression = new CalculationInfo(varRecord.DefineField,
                                                                            varRecord.InitValue?.ExecuteToIRExpression(ssaTable) as LLVMExprExpression);

                declareInfos.Add(initExpression);
            }

            blockExpression.AddItem(new LLVMLocalVarListExpression(declareInfos, ssaTable));

            // statment list (if, while, call ...)
            if (cNode.StatListNode != null)
            {
                foreach (var statement in cNode.StatListNode.StatementNodes)
                    blockExpression.AddItem(statement.ExecuteToIRExpression(ssaTable) as LLVMDependencyExpression);
            }

            return blockExpression;
        }

        private static IRExpression ReturnStatementToIRExpression(SdtsNode node, object param)
        {
            var cNode = node as ReturnStatementNode;
            var ssaTable = param as LLVMSSATable;

            return cNode.ReturnValue.ExecuteToIRExpression(ssaTable);
        }

        private static IRExpression ExprStatementNodeToIRExpression(SdtsNode node, object param)
        {
            var cNode = node as ExprStatementNode;
            var ssaTable = param as LLVMSSATable;

            return new LLVMExprStmtExpression(cNode.Expr?.ExecuteToIRExpression(ssaTable) as LLVMExprExpression, ssaTable);


            //List<LLVMLocalVariableExpression> irVarList = new List<LLVMLocalVariableExpression>();

            //foreach (var varData in cNode.SymbolTable.VarList)
            //    irVarList.Add(new LLVMLocalVariableExpression(varData.ToIRVar()));

            //LLVMBlockExpression blockExpression = new LLVMBlockExpression(irVarList);

            //if (cNode.StatListNode != null)
            //    blockExpression.AddItem(StatListNode.ToLLVMExpression());

            //return blockExpression;
        }
    }
}
