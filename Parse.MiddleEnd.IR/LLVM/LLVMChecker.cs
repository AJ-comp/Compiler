using Parse.Types;

namespace Parse.MiddleEnd.IR.LLVM
{
    public class LLVMChecker
    {
        public static bool IsExistDoubleType(StdType op1Type, StdType op2Type) => ((op1Type == StdType.Double) || (op2Type == StdType.Double));

        public static bool IsItoFpCondition(StdType op1Type, StdType op2Type)
        {
            if (op1Type == op2Type) return false;   // case double, double
            if (op1Type == StdType.Double && op2Type == StdType.Double) return false; // case not double, not double

            return true;
        }

        public static bool IsIntegerKind(StdType opType) => (opType == StdType.Byte || opType == StdType.Short || opType == StdType.Int);

        public static StdType MaximumType(StdType op1Type, StdType op2Type)
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
