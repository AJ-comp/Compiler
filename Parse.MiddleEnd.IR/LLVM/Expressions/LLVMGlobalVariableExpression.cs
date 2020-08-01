using Parse.MiddleEnd.IR.Datas;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions
{
    public class LLVMGlobalVariableExpression : LLVMFirstLayerExpression
    {
        public RootChainVar DeclaredVar { get; private set; }

        public LLVMGlobalVariableExpression(IRVar var, LLVMSSATable ssaTable) : base(ssaTable)
        {
            DeclaredVar = _ssaTable.RegisterRootChainVarToGlobal(var);
        }

        public override IEnumerable<Instruction> Build()
        {
            return new List<Instruction>() { Instruction.DeclareGlobalVar(DeclaredVar) };
        }
    }
}
