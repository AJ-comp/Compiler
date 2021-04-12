using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using Parse.Types;
using System.Collections.Generic;
using System.Linq;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions
{
    public class LLVMCastingExpression : LLVMExprExpression
    {
        public StdType CastingType { get; }
        public LLVMExprExpression Value { get; }


        public LLVMCastingExpression(StdType castingType, 
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

            if (Value.Result.TypeKind == CastingType) return instructionList;

            if (Value.Result.TypeKind == StdType.Double)
                instructionList.AddRange(ValueIsDoubleProcess(Value.Result as DoubleVariableLLVM, _ssaTable));
            else
                instructionList.AddRange(ValueIsNotDoubleProcess(Value.Result as VariableLLVM, _ssaTable));

            return instructionList;
        }

        private IEnumerable<Instruction> ValueIsNotDoubleProcess(VariableLLVM var, LLVMSSATable ssaTable)
        {
            List<Instruction> instructionList = new List<Instruction>();

            // to double
            if (CastingType == StdType.Double)
            {
                instructionList.Add(Instruction.ConvertType(var, StdType.Int, ssaTable));
                instructionList.Add(Instruction.IToFp(instructionList.First().NewSSAVar as IntVariableLLVM, ssaTable));

                return instructionList;
            }

            // to integer kind
            instructionList.Add(Instruction.ConvertType(var, CastingType, ssaTable));

            return instructionList;
        }

        private IEnumerable<Instruction> ValueIsDoubleProcess(DoubleVariableLLVM var, LLVMSSATable ssaTable)
            => new List<Instruction>() { Instruction.FpToI(var, ssaTable) };
    }
}
