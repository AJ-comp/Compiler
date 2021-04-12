using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.StmtExpressions
{
    public class LLVMEmptyStmtExpression : LLVMStmtExpression
    {
        public LLVMEmptyStmtExpression(LLVMSSATable ssaTable) : base(ssaTable)
        {
        }

        public override IEnumerable<Instruction> Build()
        {
            return new List<Instruction>();
        }
    }
}
