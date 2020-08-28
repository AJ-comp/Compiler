using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using Parse.Types;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions
{
    // constant <binop> constant can't comes because optimization logic convert to one constant.
    // ex : 10 + 15 => 25
    // so this expression always has at least one variable.
    public abstract class LLVMBinOpExpression : LLVMExprExpression
    {
        public LLVMExprExpression Left { get; protected set; }
        public LLVMExprExpression Right { get; protected set; }


        protected LLVMBinOpExpression(LLVMExprExpression left, 
                                                        LLVMExprExpression right, 
                                                        LLVMSSATable ssaTable) : base(ssaTable)
        {
            Left = left;
            Right = right;
        }


        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> result = new List<Instruction>();

            if (Left is LLVMUseVarExpression)
                (Left as LLVMUseVarExpression).IsUseVar = true;

            if (Right is LLVMUseVarExpression)
                (Right as LLVMUseVarExpression).IsUseVar = true;

            result.AddRange(Left.Build());
            result.AddRange(Right.Build());

            // Convert to equal the left type and right type.
            var toType = LLVMChecker.MaximumType(Left.Result.TypeName, Right.Result.TypeName);
            if (toType != DType.Double) toType = DType.Int;

            result.AddRange(ConvertToExtension(Left, toType));
            result.AddRange(ConvertToExtension(Right, toType));

            return result;
        }

        protected bool IsDoubleType(IValue value) => (value is IDouble);
        protected IEnumerable<Instruction> ExtensionToI32Inst(IntegerVariableLLVM value, LLVMSSATable ssTable)
        {
            List<Instruction> result = new List<Instruction>();
            if (value is IInt) return result;

            result.Add(Instruction.ConvertType(value, DType.Int, ssTable));
            return result;
        }

        protected IEnumerable<Instruction> ConvertToFpInst(IntVariableLLVM var, LLVMSSATable ssTable)
        {
            List<Instruction> result = new List<Instruction>();

            if (!IsDoubleType(Right.Result)) return result;
            if (!(Left.Result is IDouble) && !(Right.Result is IDouble)) return result;
            
            Instruction.IToFp(var, ssTable);

            return result;
        }
    }
}
