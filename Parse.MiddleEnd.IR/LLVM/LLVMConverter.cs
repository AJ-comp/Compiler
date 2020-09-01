using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions;
using Parse.Types;
using System.Collections;

namespace Parse.MiddleEnd.IR.LLVM
{
    public class LLVMConverter
    {
        public static string ToInstructionName(ReturnType type)
        {
            string result = string.Empty;

            if (type == ReturnType.Void) result = "void";
            else if (type == ReturnType.i8) result = "i8";
            else if (type == ReturnType.i16) result = "i16";
            else if (type == ReturnType.i32) result = "i32";
            else if (type == ReturnType.Double) result = "double";

            return result;
        }

        public static DType ToDType(ReturnType type)
        {
            DType result = DType.Unknown;

            if (type == ReturnType.i1) result = DType.Bit;
            else if (type == ReturnType.i8) result = DType.Byte;
            else if (type == ReturnType.i16) result = DType.Short;
            else if (type == ReturnType.i32) result = DType.Int;
            else if (type == ReturnType.Double) result = DType.Double;

            return result;
        }

        public static string ToInstructionName(DType type)
        {
            string result = string.Empty;

            if (type == DType.Bit) result = "i1";
            else if (type == DType.Byte) result = "i8";
            else if (type == DType.Short) result = "i16";
            else if (type == DType.Int) result = "i32";
            else if (type == DType.Double) result = "double";

            return result;
        }

        public static int ToBitSize(DType type) => (type == DType.Bit) ? 1 : ToAlignSize(type) * 8;

        public static int ToAlignSize(DType type)
        {
            int result = 0;

            if (type == DType.Byte) result = 1;
            else if (type == DType.Short) result = 2;
            else if (type == DType.Int) result = 4;
            else if (type == DType.Double) result = 8;

            return result;
        }

        public static string GetInstructionNameForInteger(IRCondition condition, bool bSigned)
        {
            string result = string.Empty;

            if (condition == IRCondition.EQ) result = "eq";
            else if (condition == IRCondition.NE) result = "ne";

            else if (bSigned)
            {
                if (condition == IRCondition.GT) result = "sgt";
                else if (condition == IRCondition.GE) result = "sge";
                else if (condition == IRCondition.LT) result = "slt";
                else if (condition == IRCondition.LE) result = "sle";
            }
            else
            {
                if (condition == IRCondition.GT) result = "ugt";
                else if (condition == IRCondition.GE) result = "uge";
                else if (condition == IRCondition.LT) result = "ult";
                else if (condition == IRCondition.LE) result = "ule";
            }

            return result;
        }

        public static string GetInstructionNameForDouble(IRCondition condition, bool bNans = false)
        {
            string result = string.Empty;

                if (condition == IRCondition.EQ) result = "ueq";
                else if (condition == IRCondition.NE) result = "une";

            if (bNans)
            {
                if (condition == IRCondition.GT) result = "ogt";
                else if (condition == IRCondition.GE) result = "oge";
                else if (condition == IRCondition.LT) result = "olt";
                else if (condition == IRCondition.LE) result = "ole";
            }
            else
            {
                if (condition == IRCondition.GT) result = "ugt";
                else if (condition == IRCondition.GE) result = "uge";
                else if (condition == IRCondition.LT) result = "ult";
                else if (condition == IRCondition.LE) result = "ule";
            }

            return result;
        }
    }
}
