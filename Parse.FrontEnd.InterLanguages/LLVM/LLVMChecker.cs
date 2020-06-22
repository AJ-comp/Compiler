namespace Parse.FrontEnd.InterLanguages.LLVM
{
    class LLVMChecker
    {
        public static bool IsGreater(DataType from, DataType to)
        {
            var fromAlign = LLVMConverter.ToAlign(from);
            var toAlign = LLVMConverter.ToAlign(to);

            return (fromAlign > toAlign) ? true : false;
        }

        public static bool IsDoubleType(DataType op1Type, DataType op2Type)
        {
            return (op1Type == DataType.Double || op2Type == DataType.Double) ? true : false;
        }

        public static bool IsItoFpCondition(DataType op1Type, DataType op2Type)
        {
            if (op1Type == op2Type) return false;   // case double, double
            if (op1Type != DataType.Double && op2Type != DataType.Double) return false; // case not double, not double

            return true;
        }
    }
}
