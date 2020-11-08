﻿using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions;
using System;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.StmtExpressions
{
    public class LLVMExprStmtExpression : LLVMStmtExpression
    {
        public LLVMExprStmtExpression(LLVMExprExpression expr, 
                                                        LLVMSSATable ssaTable) : base(ssaTable)
        {
            _expr = expr;
        }

        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> result = new List<Instruction>();

            if (_expr != null) result.AddRange(_expr.Build());

            return result;
        }

        private LLVMExprExpression _expr;
    }
}
