using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions
{
    public class LLVMUseVarExpression : LLVMExprExpression
    {
        public bool IsUseVar { get; set; }
        public IRVar OriginalVar { get; }
        public VariableLLVM SSAVar => Result as VariableLLVM;

        public LLVMUseVarExpression(IRVar var, LLVMSSATable ssaTable) : base(ssaTable)
        {
            OriginalVar = var;
            var localVar = _ssaTable.Find(var).LinkedObject;
            Result = localVar;
        }

        // sample format
        // %2 = load i16, i16* %1, align 2
        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> result = new List<Instruction>();

            if (IsUseVar)
            {
                var loadInst = Instruction.Load(Result as VariableLLVM, _ssaTable);
                result.AddRange(new List<Instruction>() { loadInst });
                Result = loadInst.NewSSAVar;
            }

            return result;
        }
    }
}
