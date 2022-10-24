using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using System;

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


        public static IRType MaximumType(IRType op1Type, IRType op2Type)
        {
            var op1Size = LLVMConverter.ToAlignSize(op1Type);
            var op2Size = LLVMConverter.ToAlignSize(op2Type);

            return (op1Size >= op2Size) ? op1Type : op2Type;
        }


        public static bool IsInc(IRSingleOperation operation) => operation == IRSingleOperation.PostInc || operation == IRSingleOperation.PreInc;


        /*
        public static bool Compare(ConstantIR constant1, ConstantIR constant2, IRCompareOperation compareSymbol)
        {
            if (constant1.TypeInfo.IsArithmeticType) throw new InvalidOperationException();
            if (constant2.TypeInfo.IsArithmeticType) throw new InvalidOperationException();

            var operand1 = Convert.ToDouble(constant1.Value);
            var operand2 = Convert.ToDouble(constant2.Value);

            if (compareSymbol == IRCompareOperation.EQ) return operand1 == operand2;
            else if(compareSymbol == IRCompareOperation.NE) return operand1 != operand2;
            else if (compareSymbol == IRCompareOperation.GT) return operand1 > operand2;
            else if (compareSymbol == IRCompareOperation.GE) return operand1 >= operand2;
            else if (compareSymbol == IRCompareOperation.LT) return operand1 < operand2;
            else if (compareSymbol == IRCompareOperation.LE) return operand1 <= operand2;

            throw new InvalidOperationException();
        }
        */

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
