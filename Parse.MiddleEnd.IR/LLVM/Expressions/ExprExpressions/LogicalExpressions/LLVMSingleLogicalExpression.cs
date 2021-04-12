using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types;
using Parse.Types.VarTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions.LogicalExpressions
{
    public class LLVMSingleLogicalExpression : LLVMSingleExpression
    {
        public LLVMSingleLogicalExpression(IRLogicalOpExpr expr, LLVMSSATable ssaTable) : base(expr, ssaTable)
        {
        }

        public override IEnumerable<Instruction> Build()
        {
            return base.Build();
        }

    }
}
