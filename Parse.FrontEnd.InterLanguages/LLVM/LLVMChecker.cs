using Parse.FrontEnd.InterLanguages.Datas.Types;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.LLVM.Models;
using System;

namespace Parse.MiddleEnd.IR.LLVM
{
    class LLVMChecker
    {
        public static bool IsExistDoubleType(DataType op1Type, DataType op2Type) => ((op1Type is DoubleType) || (op2Type is DoubleType));

        public static bool IsItoFpCondition(DataType op1Type, DataType op2Type)
        {
            if (op1Type == op2Type) return false;   // case double, double
            if (op1Type is DoubleType && op2Type is DoubleType) return false; // case not double, not double

            return true;
        }
    }
}
