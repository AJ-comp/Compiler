using System;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions
{
    public class LLVMIFExpression : LLVMDependencyExpression
    {
        public LLVMIFExpression(LLVMSSATable ssaTable) : base(ssaTable)
        {
        }

        public override IEnumerable<Instruction> Build()
        {
            throw new NotImplementedException();
        }
    }
}
