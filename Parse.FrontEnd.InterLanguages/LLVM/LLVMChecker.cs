using Parse.MiddleEnd.IR.Datas;

namespace Parse.MiddleEnd.IR.LLVM
{
    class LLVMChecker
    {
        public static bool IsGreater(DataType from, DataType to)
        {
            var fromAlign = LLVMConverter.ToAlign(from);
            var toAlign = LLVMConverter.ToAlign(to);

            return (fromAlign > toAlign);
        }

        public static bool IsDoubleType(DataType op1Type, DataType op2Type)
        {
            return (op1Type == DataType.Double || op2Type == DataType.Double);
        }

        public static bool IsSigned(IRData op1, IRData op2) => (op1.IsSigned && op2.IsSigned);

        public static bool IsNans(IRData op1, IRData op2) => (op1.IsNan && op2.IsNan);

        public static bool IsExistDoubleType(IRData op1, IRData op2) => (op1.Type == DataType.Double || op2.Type == DataType.Double);

        public static bool IsItoFpCondition(DataType op1Type, DataType op2Type)
        {
            if (op1Type == op2Type) return false;   // case double, double
            if (op1Type != DataType.Double && op2Type != DataType.Double) return false; // case not double, not double

            return true;
        }
    }
}
