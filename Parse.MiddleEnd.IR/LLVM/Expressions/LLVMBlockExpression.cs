using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.LLVM.Expressions
{
    public class LLVMBlockExpression : LLVMExpression
    {
        public IReadOnlyList<LLVMExpression> Expressions => _expressions;

        public LLVMBlockExpression(IReadOnlyList<LLVMExpression> expressions)
        {
            _expressions.AddRange(expressions);
        }


        private List<LLVMExpression> _expressions;

        public override string GeneratedCode()
        {
            throw new NotImplementedException();
        }
    }
}
