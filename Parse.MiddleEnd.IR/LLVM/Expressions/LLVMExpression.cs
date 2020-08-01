using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions
{
    public abstract class LLVMExpression : IRExpression
    {
        public List<LLVMExpression> Items = new List<LLVMExpression>();

        public override string ToString() => GetType().Name;

        public abstract IEnumerable<Instruction> Build();

        protected LLVMSSATable _ssaTable;

        protected LLVMExpression(LLVMSSATable ssaTable)
        {
            _ssaTable = ssaTable;
        }
    }
}
