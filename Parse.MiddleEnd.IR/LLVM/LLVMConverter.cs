﻿using AJ.Common.Helpers;
using Parse.Extensions;
using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using System.Linq;

namespace Parse.MiddleEnd.IR.LLVM
{
    public class LLVMConverter
    {
        public static string ToInstructionName(StdType type)
        {
            string result = string.Empty;

            if (type == StdType.Void) result = "void";
            else if (type == StdType.Bit) result = "i1";
            else if (type == StdType.Char) result = "i8";
            else if (type == StdType.Short) result = "i16";
            else if (type == StdType.Int) result = "i32";
            else if (type == StdType.Double) result = "double";
            else if (type == StdType.Struct) result = "%struct";

            return result;
        }

        public static string ToInstructionName(OperatorCode operation)
        {
            string result = string.Empty;

            if (operation == OperatorCode.Add) result = "add";
            else if (operation == OperatorCode.Sub) result = "sub";
            else if (operation == OperatorCode.Mul) result = "mul";
            else if (operation == OperatorCode.Div) result = "sdiv";
            else if (operation == OperatorCode.Mod) result = "srem";

            return result;
        }

        public static string ToType(TypeInfo typeInfo)
        {
            string typeName = string.Empty;

            if (typeInfo.Type == StdType.Struct) typeName = $".{typeInfo.Name}";

            string arrayHeader = string.Empty;
            var result = $"{ToInstructionName(typeInfo.Type)}{typeName}{typeInfo.PointerLevel.ToAnyStrings("*")}";

            foreach (var arrayLength in typeInfo.ArrayLengths)
            {
                arrayHeader = $"[ {arrayLength} x ";
            }

            string arrayTailer = "]".RepeatString(typeInfo.ArrayLengths.Count());

            return $"{arrayHeader}{result}{arrayTailer}";
        }


        public static uint ToAlignSize(StdType type)
        {
            uint result = 0;

            if (type == StdType.Char) result = 1;
            else if (type == StdType.Short) result = 2;
            else if (type == StdType.Int) result = 4;
            else if (type == StdType.Double) result = 8;

            return result;
        }

        public static uint ToAlignSize(TypeInfo typeInfo) => (typeInfo.PointerLevel > 0) ? 8 : ToAlignSize(typeInfo.Type);

        public static string GetInstructionNameForInteger(IRCompareOperation condition, bool bSigned)
        {
            string result = string.Empty;

            if (condition == IRCompareOperation.EQ) result = "eq";
            else if (condition == IRCompareOperation.NE) result = "ne";

            else if (bSigned)
            {
                if (condition == IRCompareOperation.GT) result = "sgt";
                else if (condition == IRCompareOperation.GE) result = "sge";
                else if (condition == IRCompareOperation.LT) result = "slt";
                else if (condition == IRCompareOperation.LE) result = "sle";
            }
            else
            {
                if (condition == IRCompareOperation.GT) result = "ugt";
                else if (condition == IRCompareOperation.GE) result = "uge";
                else if (condition == IRCompareOperation.LT) result = "ult";
                else if (condition == IRCompareOperation.LE) result = "ule";
            }

            return result;
        }

        public static string GetInstructionNameForDouble(IRCompareOperation condition, bool bNans = false)
        {
            string result = string.Empty;

            if (condition == IRCompareOperation.EQ) result = "ueq";
            else if (condition == IRCompareOperation.NE) result = "une";

            if (bNans)
            {
                if (condition == IRCompareOperation.GT) result = "ogt";
                else if (condition == IRCompareOperation.GE) result = "oge";
                else if (condition == IRCompareOperation.LT) result = "olt";
                else if (condition == IRCompareOperation.LE) result = "ole";
            }
            else
            {
                if (condition == IRCompareOperation.GT) result = "ugt";
                else if (condition == IRCompareOperation.GE) result = "uge";
                else if (condition == IRCompareOperation.LT) result = "ult";
                else if (condition == IRCompareOperation.LE) result = "ule";
            }

            return result;
        }

        public static string GetMCPUOption(string name)
        {
            return (name.Contains("Stm32")) ? "cortex-m3" : string.Empty;
        }
    }
}
