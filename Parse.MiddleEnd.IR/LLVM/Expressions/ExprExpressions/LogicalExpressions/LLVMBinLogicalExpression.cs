using Parse.MiddleEnd.IR.Interfaces;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions.LogicalExpressions
{
    public abstract class LLVMBinLogicalExpression : LLVMBinOpExpression
    {
        public LLVMBinLogicalExpression(IRCompareOpExpr expression, LLVMSSATable ssaTable) : base(expression, ssaTable)
        {
        }

        public override IEnumerable<Instruction> Build() => base.Build();
    }
}
