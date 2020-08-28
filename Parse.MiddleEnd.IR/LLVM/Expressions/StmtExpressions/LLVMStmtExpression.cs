using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.StmtExpressions
{
    public abstract class LLVMStmtExpression : LLVMDependencyExpression
    {
        protected LLVMStmtExpression(LLVMSSATable ssaTable) : base(ssaTable)
        {
        }
    }
}
