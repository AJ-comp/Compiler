using Parse.MiddleEnd.IR.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions
{
    public abstract class LLVMSingleExpression : LLVMExprExpression
    {
        protected LLVMSingleExpression(IRExpr expr, LLVMSSATable ssaTable) : base(ssaTable)
        {
            _expr = Create(expr, ssaTable);
        }

        public override IEnumerable<Instruction> Build()
        {
            return _expr.Build();
        }


        protected LLVMExprExpression _expr;
    }
}
