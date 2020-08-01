using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using Parse.Types;
using System.Collections.Generic;
using System.Linq;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions
{
    public class LLVMCastingExpression : LLVMExprExpression
    {
        public DType CastingType { get; }
        public LLVMExprExpression Value { get; }


        public LLVMCastingExpression(DType castingType, 
                                                    LLVMExprExpression valueToCasting, 
                                                    LLVMSSATable ssaTable) : base(ssaTable)
        {
            CastingType = castingType;
            Value = valueToCasting;
        }


        // sample format
        // [<result> = sext <ty> <op> to i32]*
        // || [<result> = sitofp i32 <op> to double] || [<result> = uitofp i32 <op> to double]
        // || [<result> = fptosi double <op> to i32] || [<result> = fptoui double <op> to i32]
        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> instructionList = new List<Instruction>();

            if (Value.Result.TypeName == CastingType) return instructionList;

            if (Value.Result is IntegerVariableLLVM)
                instructionList.AddRange(ValueIsIntegerProcess(Value.Result as IntegerVariableLLVM, _ssaTable));
            else if (CastingType == DType.Double)
                instructionList.AddRange(ValueIsDoubleProcess(Value.Result as DoubleVariableLLVM, _ssaTable));

            return instructionList;
        }

        private IEnumerable<Instruction> ValueIsIntegerProcess(IntegerVariableLLVM var, LLVMSSATable ssTable)
        {
            List<Instruction> instructionList = new List<Instruction>();

            // to double
            if (CastingType == DType.Double)
            {
                instructionList.Add(Instruction.ConvertType(var, DType.Int, ssTable));
                instructionList.Add(Instruction.IToFp(instructionList.First().NewSSAVar as IntVariableLLVM, ssTable));

                return instructionList;
            }

            // to integer kind
            instructionList.Add(Instruction.ConvertType(var, CastingType, ssTable));

            return instructionList;
        }

        private IEnumerable<Instruction> ValueIsDoubleProcess(DoubleVariableLLVM var, LLVMSSATable ssTable)
            => new List<Instruction>() { Instruction.FpToI(var, ssTable) };
    }
}
