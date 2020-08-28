using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions.LogicalExpressions;
using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using System;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.StmtExpressions
{
    public class LLVMIFExpression : LLVMStmtExpression
    {
        public LLVMIFExpression(LLVMLogicalOpExpression condExpression, 
                                            LLVMStmtExpression stmtExpression, 
                                            LLVMSSATable ssaTable) : base(ssaTable)
        {
            _condExpression = condExpression;
            _stmtExpression = stmtExpression;
        }

        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> result = new List<Instruction>();

            // process 1
            result.AddRange(_condExpression.Build());

            #region process 3
            List<Instruction> reservedInsts = new List<Instruction>();

            // create a true label.
            var trueLabel = _ssaTable.RegisterLabel();
            reservedInsts.Add(Instruction.EmptyLine());
            reservedInsts.Add(Instruction.EmptyLine(trueLabel.Name));
            reservedInsts.AddRange(_stmtExpression.Build());

            // create a next label
            var nextLabel = _ssaTable.RegisterLabel();
            reservedInsts.Add(Instruction.EmptyLine());
            reservedInsts.Add(Instruction.EmptyLine(nextLabel.Name));
            reservedInsts.Add(Instruction.UCBranch(nextLabel));
            #endregion

            // process 2
            result.Add(Instruction.CBranch(_condExpression.Result as BitVariableLLVM,
                                                            trueLabel,
                                                            nextLabel));

            result.AddRange(reservedInsts);

            return result;
        }

        private LLVMLogicalOpExpression _condExpression;
        private LLVMStmtExpression _stmtExpression;
    }
}
