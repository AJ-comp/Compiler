using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.StatementNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables;
using Parse.MiddleEnd.IR;
using Parse.MiddleEnd.IR.LLVM;
using Parse.MiddleEnd.IR.LLVM.Expressions;
using Parse.MiddleEnd.IR.LLVM.Expressions.AssignExpressions;
using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions;

namespace Parse.FrontEnd.IRGenerator
{
    public partial class IRExpressionGenerator
    {
        private static IRExpression CommonCondStatementToIRExpression(SdtsNode node, object param)
        {
            var cNode = node as CondStatementNode;
            var ssaTable = param as LLVMSSATable;

            LLVMBlockExpression result = new LLVMBlockExpression(ssaTable);

            result.AddItem(cNode.Condition.ExecuteToIRExpression(ssaTable) as LLVMExpression);
            result.AddItem(cNode.TrueStatement.ExecuteToIRExpression(ssaTable) as LLVMExpression);

            return result;
        }

        private static IRExpression CompoundStNodeToIRExpression(SdtsNode node, object param)
        {
            CompoundStNode cNode = node as CompoundStNode;
            var ssaTable = param as LLVMSSATable;
            LLVMBlockExpression blockExpression = new LLVMBlockExpression(ssaTable);

            // local variable declaration
            foreach (var varRecord in cNode.SymbolTable.VarList)
                blockExpression.AddItem(new LLVMLocalVariableExpression(varRecord.VarField, ssaTable));

            // local variable initialize
            foreach (var varRecord in cNode.SymbolTable.VarList)
            {
                if (varRecord.VarField.VariableProperty == VariableMiniC.VarProperty.Param) continue;
                if (varRecord.InitValue.IsInitialized)
                    blockExpression.AddItem(new LLVMAssignExpression(varRecord.VarField, 
                                                                                                    new LLVMConstantExpression(varRecord.InitValue, ssaTable),
                                                                                                    ssaTable));
            }

            // statment list (if, while, call ...)
            if (cNode.StatListNode != null)
            {
                foreach (var statement in cNode.StatListNode.StatementNodes)
                    blockExpression.AddItem(statement.ExecuteToIRExpression(ssaTable) as LLVMDependencyExpression);
            }

            return blockExpression;
        }

        private static IRExpression IfElseStatementToIRExpression(SdtsNode node, object param)
        {
            var cNode = node as IfElseStatementNode;
            var ssaTable = param as LLVMSSATable;

            var result = CommonCondStatementToIRExpression(node, ssaTable) as LLVMBlockExpression;
            result.AddItem(cNode.FalseStatement.ExecuteToIRExpression(ssaTable) as LLVMExpression);

            return result;
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

            return cNode.Expr.ExecuteToIRExpression(ssaTable);


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
