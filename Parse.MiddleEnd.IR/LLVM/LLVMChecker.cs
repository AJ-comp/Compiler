using Parse.Types;

namespace Parse.MiddleEnd.IR.LLVM
{
    public class LLVMChecker
    {
        public static bool IsExistDoubleType(DType op1Type, DType op2Type) => ((op1Type == DType.Double) || (op2Type == DType.Double));

        public static bool IsItoFpCondition(DType op1Type, DType op2Type)
        {
            if (op1Type == op2Type) return false;   // case double, double
            if (op1Type == DType.Double && op2Type == DType.Double) return false; // case not double, not double

            return true;
        }

        public static bool IsIntegerKind(DType opType) => (opType == DType.Byte || opType == DType.Short || opType == DType.Int);

        public static DType MaximumType(DType op1Type, DType op2Type)
        {
            var op1Size = LLVMConverter.ToAlignSize(op1Type);
            var op2Size = LLVMConverter.ToAlignSize(op2Type);

            return (op1Size >= op2Size) ? op1Type : op2Type;
        }

        //public static BitConstantLLVM LogicalOp(IRValue s, IRValue t, IRCondition condition)
        //{
        //    if (condition == IRCondition.EQ) return new BitConstantLLVM(s.IsEqual(t));
        //    if (condition == IRCondition.NE) return new BitConstantLLVM(!s.IsEqual(t));
        //    if (condition == IRCondition.GT) return new BitConstantLLVM(s.IsGreaterThan(t));
        //    if (condition == IRCondition.GE) return new BitConstantLLVM(!s.IsLessThan(t));
        //    if (condition == IRCondition.LT) return new BitConstantLLVM(s.IsLessThan(t));
        //    if (condition == IRCondition.LE) return new BitConstantLLVM(!s.IsGreaterThan(t));

        //    return new BitConstantLLVM(false);
        //}
    }
}
