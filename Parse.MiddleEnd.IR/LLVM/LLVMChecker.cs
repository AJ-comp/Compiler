using Parse.MiddleEnd.IR.Datas.Types;

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
    }
}
