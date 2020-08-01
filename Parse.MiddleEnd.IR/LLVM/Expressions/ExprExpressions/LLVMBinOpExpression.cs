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


        protected IEnumerable<Instruction> ArithmeticPrevProcess(LLVMExprExpression target, LLVMExprExpression toCompare)
        {
            // Bit type never can come to this because it is filtered by semantic analysis module.

            List<Instruction> result = new List<Instruction>();
            if (target.Result.TypeName == toCompare.Result.TypeName) return result;

            if (LLVMChecker.IsIntegerKind(target.Result.TypeName))
            {
                if (target.Result.TypeName != DType.Int)
                {
                    var casting = new LLVMCastingExpression(DType.Int, target, _ssaTable);
                    result.AddRange(casting.Build());
                }
            }

            if (toCompare.Result.TypeName == DType.Double)
            {
                var casting = new LLVMCastingExpression(DType.Double, target, _ssaTable);
                result.AddRange(casting.Build());
            }

            return result;
        }
    }
}
