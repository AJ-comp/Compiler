using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions.UseVarExpressions
{
    public class LLVMUseNormalVarExpression : LLVMUseVarExpression
    {
        public IRDeclareVar OriginalVar { get; }
        public VariableLLVM SSAVar => Result as VariableLLVM;

        public LLVMUseNormalVarExpression(IRDeclareVar var, LLVMSSATable ssaTable) : base(ssaTable)
        {
            OriginalVar = var;
            Result = _ssaTable.Find(var).LinkedObject;
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



        private IEnumerable<Instruction> BuildForMemberUseVar()
        {
            List<Instruction> result = new List<Instruction>();

            if (OriginalVar.IsMember)
            {

            }

            return result;
        }
    }
}
