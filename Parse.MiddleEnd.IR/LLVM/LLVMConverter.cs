using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions;
using Parse.Types;
using System.Collections;

namespace Parse.MiddleEnd.IR.LLVM
{
    public class LLVMConverter
    {
        public static string ToInstructionName(StdType type)
        {
            string result = string.Empty;

            if (type == StdType.Void) result = "void";
            else if (type == StdType.Bit) result = "i1";
            else if (type == StdType.Byte) result = "i8";
            else if (type == StdType.Short) result = "i16";
            else if (type == StdType.Int) result = "i32";
            else if (type == StdType.Double) result = "double";
            else if (type == StdType.Struct) result = "%struct";

            return result;
        }

        public static string ToInstructionName(IROperation operation)
        {
            string result = string.Empty;

            if (operation == IROperation.Add) result = "add";
            else if (operation == IROperation.Sub) result = "sub";
            else if (operation == IROperation.Mul) result = "mul";
            else if (operation == IROperation.Div) result = "sdiv";
            else if (operation == IROperation.Mod) result = "srem";

            return result;
        }

        public static string ToInstructionName(IRDeclareVar var)
        {
            if (var is IRDeclareStructTypeVar)
            {
                var userDefVar = var as IRDeclareStructTypeVar;

                return string.Format("{0}.{1}{2}",
                                                ToInstructionName(var.TypeKind),
                                                userDefVar.TypeName,
                                                ToPointerDepth(var.PointerLevel));
            }

            return string.Format("{0}{1}",
                                            ToInstructionName(var.TypeKind),
                                            ToPointerDepth(var.PointerLevel));
        }

        public static string ToPointerDepth(uint pointerLevel)
        {
            string result = string.Empty;

            for (int i = 0; i < pointerLevel; i++) result += "*";

            return result;
        }

        public static int ToAlignSize(StdType typeName)
        {
            int result = 0;

            if (typeName == StdType.Byte) result = 1;
            else if (typeName == StdType.Short) result = 2;
            else if (typeName == StdType.Int) result = 4;
            else if (typeName == StdType.Double) result = 8;

            return result;
        }

        public static int ToAlignSize(IRDeclareVar var)
        {
            if (var.PointerLevel > 0) return 8;

            return ToAlignSize(var.TypeKind);
        }

        public static string GetInstructionNameForInteger(IRCompareSymbol condition, bool bSigned)
        {
            string result = string.Empty;

            if (condition == IRCompareSymbol.EQ) result = "eq";
            else if (condition == IRCompareSymbol.NE) result = "ne";

            else if (bSigned)
            {
                if (condition == IRCompareSymbol.GT) result = "sgt";
                else if (condition == IRCompareSymbol.GE) result = "sge";
                else if (condition == IRCompareSymbol.LT) result = "slt";
                else if (condition == IRCompareSymbol.LE) result = "sle";
            }
            else
            {
                if (condition == IRCompareSymbol.GT) result = "ugt";
                else if (condition == IRCompareSymbol.GE) result = "uge";
                else if (condition == IRCompareSymbol.LT) result = "ult";
                else if (condition == IRCompareSymbol.LE) result = "ule";
            }

            return result;
        }

        public static string GetInstructionNameForDouble(IRCompareSymbol condition, bool bNans = false)
        {
            string result = string.Empty;

            if (condition == IRCompareSymbol.EQ) result = "ueq";
            else if (condition == IRCompareSymbol.NE) result = "une";

            if (bNans)
            {
                if (condition == IRCompareSymbol.GT) result = "ogt";
                else if (condition == IRCompareSymbol.GE) result = "oge";
                else if (condition == IRCompareSymbol.LT) result = "olt";
                else if (condition == IRCompareSymbol.LE) result = "ole";
            }
            else
            {
                if (condition == IRCompareSymbol.GT) result = "ugt";
                else if (condition == IRCompareSymbol.GE) result = "uge";
                else if (condition == IRCompareSymbol.LT) result = "ult";
                else if (condition == IRCompareSymbol.LE) result = "ule";
            }

            return result;
        }

        public static string GetMCPUOption(string name)
        {
            return (name.Contains("Stm32")) ? "cortex-m3" : string.Empty;
        }
    }
}
