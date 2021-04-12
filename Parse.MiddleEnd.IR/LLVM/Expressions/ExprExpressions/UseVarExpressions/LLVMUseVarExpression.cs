using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions.UseVarExpressions
{
    public abstract class LLVMUseVarExpression : LLVMExprExpression
    {
        public bool IsUseVar { get; set; }

        protected LLVMUseVarExpression(LLVMSSATable ssaTable) : base(ssaTable)
        {
        }
    }
}
