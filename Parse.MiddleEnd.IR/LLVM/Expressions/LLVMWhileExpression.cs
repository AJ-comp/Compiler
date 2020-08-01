using System;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions
{
    public class LLVMWhileExpression : LLVMDependencyExpression
    {
        public LLVMWhileExpression(LLVMSSATable ssaTable) : base(ssaTable)
        {
        }

        public override IEnumerable<Instruction> Build()
        {
            throw new NotImplementedException();
        }
    }
}
