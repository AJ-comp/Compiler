using Parse.MiddleEnd.IR.Datas.Types;
using Parse.MiddleEnd.IR.Datas.ValueDatas;
using Parse.MiddleEnd.IR.LLVM.Models;

namespace Parse.MiddleEnd.IR.LLVM
{
    class LLVMChecker
    {
        public static bool IsExistDoubleType(DType op1Type, DType op2Type) => ((op1Type == DType.Double) || (op2Type == DType.Double));

        public static bool IsItoFpCondition(DType op1Type, DType op2Type)
        {
            if (op1Type == op2Type) return false;   // case double, double
            if (op1Type == DType.Double && op2Type == DType.Double) return false; // case not double, not double

            return true;
        }

        public static IRValue<Bit> LogicalOp(IRValue s, IRValue t, IRCondition condition)
        {
            if (condition == IRCondition.EQ) return new SSValue<Bit>(s.IsEqual(t));
            if (condition == IRCondition.NE) return new SSValue<Bit>(!s.IsEqual(t));
            if (condition == IRCondition.GT) return new SSValue<Bit>(s.IsGreaterThan(t));
            if (condition == IRCondition.GE) return new SSValue<Bit>(!s.IsLessThan(t));
            if (condition == IRCondition.LT) return new SSValue<Bit>(s.IsLessThan(t));
            if (condition == IRCondition.LE) return new SSValue<Bit>(!s.IsGreaterThan(t));

            return new SSValue<Bit>(false);
        }
    }
}
