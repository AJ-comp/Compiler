using Parse.MiddleEnd.IR.Datas;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions
{
    public class LLVMLocalVariableExpression : LLVMDependencyExpression
    {
        public RootChainVar DeclaredVar { get; private set; }

        public LLVMLocalVariableExpression(IRVar var, LLVMSSATable ssaTable) : base(ssaTable)
        {
            DeclaredVar = _ssaTable.RegisterRootChainVarToLocal(var);
        }

        public override IEnumerable<Instruction> Build()
        {
            return new List<Instruction>() { Instruction.DeclareLocalVar(DeclaredVar) };
        }
    }
}
