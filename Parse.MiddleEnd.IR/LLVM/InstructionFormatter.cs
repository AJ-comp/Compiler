using Parse.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Parse.MiddleEnd.IR.LLVM
{
    /*
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public partial class InstructionFormatter : IRUnit
    {
        public string CommandLine { get; }
        public string Comment => (_comment.Length > 0) ? ";" + _comment : string.Empty;

        public string FullData => CommandLine + Comment;

        public InstructionFormatter(string command, string comment = "")
        {
            CommandLine = command;
            _comment = comment;
        }


        internal static string DeclareLocalVar(IRVar var, string comment = "")
        {
            uint align = LLVMConverter.ToAlignSize(var.TypeInfo);
            var type = LLVMConverter.ToType(var.TypeInfo);

            return $"{var.NameForLLVM} = alloca {type}, align {align}";
        }


        // sample
        // <result> = load <ty>, <ty>* <pointer>[, align <alignment>]
        internal static string Load(IRVar newVar, IRVar useVar)
        {
            uint align = LLVMConverter.ToAlignSize(newVar.TypeInfo);
            var type = LLVMConverter.ToType(newVar.TypeInfo);

            return $"{newVar.NameForLLVM} = load {type}, {type}* {useVar.NameForLLVM}, align {align}";
        }
    */


        /*************************************************/
        /// <summary>
        /// sample
        /// store i32 %3, i32* @a, align 4
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        /*************************************************/
        /*
        internal static string Store(IRVar from, IRVar to, string comment = "")
        {
            if (from.TypeInfo.PointerLevel != to.TypeInfo.PointerLevel - 1) throw new FormatException();

            uint fromAlign = LLVMConverter.ToAlignSize(from.TypeInfo);
            var fromType = LLVMConverter.ToType(to.TypeInfo);

            return string.Format("store {0} {1}, {0}* {2}, align {3}",
                                            fromType, from.NameForLLVM, to.NameForLLVM, fromAlign); // string param
        }
        */


        /*************************************************/
        /// <summary>
        /// store i32 10, i32* %1, align 4
        /// </summary>
        /// <remarks>
        /// 모호성 오류를 피하기 위해 파라메터 순서를 바꿉니다.
        /// </remarks>
        /// <param name="deRef"></param>
        /// <param name="from"></param>
        /// <param name="toVar"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        /*************************************************/
        /*
        internal static string Store(SqlModels.ConstantIR from, IRVar toVar, string comment = "")
        {
            uint fromAlign = LLVMConverter.ToAlignSize(toVar.TypeInfo);
            var toType = LLVMConverter.ToType(toVar.TypeInfo);

            string valueString = (toVar.TypeInfo.PointerLevel > 0)
                                      ? $"inttoptr (i64 {from.Value} to {toType})" : from.Value;

            return $"store {toType} {valueString}, {toType}* {toVar.NameForLLVM}, align {fromAlign}";
        }

        internal static string BinOp(IRVar newVar, IRVar op1, SqlModels.ConstantIR op2, OperatorCode operation)
            => BinOp(newVar, op1, op2.Value, operation);

        // sample
        // <result> = add nsw <ty> <op1>, <op2>
        internal static string BinOp(IRVar newVar, IRVar op1, IRVar op2, OperatorCode operation)
        {
            var binOp = LLVMConverter.ToInstructionName(operation);

            if (op1.TypeInfo.Type == StdType.Double)
            {
                return $"{newVar.NameForLLVM} = f{binOp} double {op1.NameForLLVM}, {op2.NameForLLVM}";
            }
            else
            {
                var type = LLVMConverter.ToInstructionName(op1.TypeInfo.Type);

                return $"{newVar.NameForLLVM} = {binOp} nsw {type} {op1.NameForLLVM}, {op2.NameForLLVM}";
            }
        }

        internal static string BinOp(IRVar newVar, IRVar op1, string op2, OperatorCode operation)
        {
            var binOp = LLVMConverter.ToInstructionName(operation);

            if (op1.TypeInfo.Type == StdType.Double)
            {
                return $"{newVar.NameForLLVM} = f{binOp} double {op1.NameForLLVM}, {op2}";
            }
            else
            {
                var type = LLVMConverter.ToInstructionName(op1.TypeInfo.Type);

                return $"{newVar.NameForLLVM} = {binOp} nsw {type} {op1.NameForLLVM}, {op2}";
            }
        }

        // sample
        // <result> = sext <ty> <op> to <to_op> || <result> = trunc <ty> <op> to <to_op>
        internal static string ExtOrTrunc(IRVar toVar, IRVar op)
        {
            var typeSize = LLVMConverter.ToAlignSize(op.TypeInfo);
            var toTypeSize = LLVMConverter.ToAlignSize(toVar.TypeInfo);

            if (toVar.TypeInfo.Type == StdType.Double) return string.Empty;
            if (typeSize == toTypeSize) return string.Empty;

            var typeName = LLVMConverter.ToType(op.TypeInfo);
            var toTypeName = LLVMConverter.ToType(toVar.TypeInfo);

            if (typeSize < toTypeSize)
            {
                var command = (typeSize == 0) ? "zext" : "sext";

                return $"{toVar.NameForLLVM} = {command} {typeName} {op.NameForLLVM} to {toTypeName}";
            }
            else
            {
                return $"{toVar.NameForLLVM} = trunc {toTypeName} {op.NameForLLVM} to {typeName}";
            }
        }

        // <result> = sitofp i32 <op> to double
        // or
        // <result> = uitofp i32 <op> to double
        internal static string IToFp(IRVar toVar, IRVar var)
        {
            if (var.TypeInfo.Type != StdType.Int) throw new ArgumentException();
            if (toVar.TypeInfo.Type != StdType.Double) throw new ArgumentException();

            var toFP = (var.TypeInfo.Signed) ? "sitofp" : "uitofp";

            return $"{toVar.NameForLLVM} = {toFP} i32 {var.NameForLLVM} to i32";
        }

        internal static string FpToI(IRVar var)
        {
            throw new Exception();
        }

        // sample (ICMP Abstract)
        // <result> = icmp <cond> <ty> <op1>, <op2>
        internal static string IcmpA(IRCompareOperation cond, IRVar newVar, IRValue op1, IRValue op2)
        {
            if (newVar.TypeInfo.Type != StdType.Bit) throw new FormatException();
            if (op1.TypeInfo.Type != StdType.Int) throw new FormatException();
            if (op2.TypeInfo.Type != StdType.Int) throw new FormatException();

            return (op1 is IRVar && op2 is IRVar) ? Icmp(cond, newVar, op1 as IRVar, op2 as IRVar) :
                      (op1 is IRVar && op2 is SqlModels.ConstantIR) ? Icmp(cond, newVar, op1 as IRVar, op2 as SqlModels.ConstantIR) :
                      (op1 is SqlModels.ConstantIR && op2 is IRVar) ? Icmp(cond, newVar, op2 as IRVar, op1 as SqlModels.ConstantIR)
                                                                             : throw new FormatException();
        }

        // sample
        // <result> = icmp <cond> <ty> <op1>, <op2>
        internal static string Icmp(IRCompareOperation cond, IRVar newVar, IRVar op1, IRVar op2)
        {
            var isAllSigned = (op1.TypeInfo.Signed && op2.TypeInfo.Signed);
            var typeName = LLVMConverter.ToInstructionName(op1.TypeInfo.Type);
            var condIns = LLVMConverter.GetInstructionNameForInteger(cond, isAllSigned);

            return $"{newVar.NameForLLVM} = icmp {condIns} {typeName} {op1.NameForLLVM}, {op2.NameForLLVM}";
        }

        // sample
        // <result> = icmp <cond> <ty> <op1>, <op2>
        internal static string Icmp(IRCompareOperation cond, IRVar newVar, IRVar op1, SqlModels.ConstantIR op2)
        {
            var isSigned = (op1.TypeInfo.Signed && op2.TypeInfo.Signed);
            var typeName = LLVMConverter.ToInstructionName(op1.TypeInfo.Type);
            var condIns = LLVMConverter.GetInstructionNameForInteger(cond, isSigned);

            return $"{newVar.NameForLLVM} = icmp {condIns} {typeName} {op1.NameForLLVM}, {op2.Value}";
        }

        // sample (FCMP Abstract)
        // <result> = fcmp[fast - math flags]* <cond> <ty> <op1>, <op2>
        internal static string FcmpA(IRCompareOperation cond, IRVar newVar, IRValue op1, IRValue op2)
        {
            if (newVar.TypeInfo.Type != StdType.Bit) throw new FormatException();
            if (op1.TypeInfo.Type != StdType.Double) throw new FormatException();
            if (op2.TypeInfo.Type != StdType.Double) throw new FormatException();

            return (op1 is IRVar && op2 is IRVar) ? Fcmp(cond, newVar, op1 as IRVar, op2 as IRVar) :
                      (op1 is IRVar && op2 is SqlModels.ConstantIR) ? Fcmp(cond, newVar, op1 as IRVar, op2 as SqlModels.ConstantIR) :
                      (op1 is SqlModels.ConstantIR && op2 is IRVar) ? Fcmp(cond, newVar, op2 as IRVar, op1 as SqlModels.ConstantIR)
                                                                                       : throw new FormatException();
        }

        // sample
        // <result> = fcmp[fast - math flags]* <cond> <ty> <op1>, <op2>
        internal static string Fcmp(IRCompareOperation cond, IRVar newVar, IRVar op1, IRVar op2)
        {
            var isNan = (op1.TypeInfo.Nan && op2.TypeInfo.Nan);
            var typeName = LLVMConverter.ToInstructionName(op1.TypeInfo.Type);
            var condIns = LLVMConverter.GetInstructionNameForDouble(cond, isNan);

            return $"{newVar.NameForLLVM} = fcmp {condIns} {typeName} {op1.NameForLLVM}, {op2.NameForLLVM}";
        }

        // sample
        // <result> = fcmp[fast - math flags]* <cond> <ty> <op1>, <op2>
        internal static string Fcmp(IRCompareOperation cond, IRVar newVar, IRVar op1, SqlModels.ConstantIR op2)
        {
            var isNan = (op1.TypeInfo.Nan && op2.TypeInfo.Nan);
            var typeName = LLVMConverter.ToInstructionName(op1.TypeInfo.Type);
            var condIns = LLVMConverter.GetInstructionNameForDouble(cond, isNan);

            return $"{newVar.NameForLLVM} = fcmp {condIns} {typeName} {op1.NameForLLVM}, {op2.Value}";
        }

        // sample
        // br i1 <condlabel>, label <iftrue>, label <iffalse>
        internal static string CBranch(IRVar cond, IRVar trueLabel, IRVar falseLabel)
        {
            if (cond.TypeInfo.Type != StdType.Bit) throw new Exception();
            if (trueLabel.TypeInfo.Type != StdType.Bit) throw new Exception();
            if (falseLabel.TypeInfo.Type != StdType.Bit) throw new Exception();

            return $"br i1 {cond.NameForLLVM}, label {trueLabel.NameForLLVM}, label {falseLabel.NameForLLVM}";
        }

        // sample
        // br label <dest>
        internal static string UCBranch(IRVar label) => $"br label {label.NameForLLVM}";

        internal static string GetElement(IRVar newVar, IRVar loadedVar)
        {
            List<string> result = new List<string>();

            var typeName = LLVMConverter.ToType(loadedVar.TypeInfo);
//            if (loadedVar.AllocaVar == loadedVar) throw new FormatException();

            return string.Format("{0} = getelementptr inbounds {1}, {1}* {2}, i32 0 i32 {3}",
                                            newVar.NameForLLVM, typeName, loadedVar.NameForLLVM, loadedVar.Offset);
        }

        internal static string Call(Function irFuncData, IReadOnlyList<IRValue> passedParams, LLVMSSATable ssaTable)
        {
            string returnCommand = string.Empty;
            var returnType = irFuncData.GetReturnTypeInfo();

            if (returnType.Type != StdType.Void)
            {
                var resultVar = ssaTable.NewLink(irFuncData.GetReturnTypeInfo());
                returnCommand = resultVar.Name;
            }

            string command = $"call {LLVMConverter.ToInstructionName(returnType.Type)} @{irFuncData.Name}";

            string argData = "(";
            for (int i = 0; i < passedParams.Count; i++)
            {
                var passedParam = passedParams[i];

                argData += LLVMConverter.ToInstructionName(passedParam.TypeInfo.Type) + " ";

                if (passedParam is SqlModels.ConstantIR)
                    argData += (passedParam as SqlModels.ConstantIR).Value;
                else if (passedParam is IRVar)
                    argData += (passedParam as IRVar).NameForLLVM;
                else throw new FormatException();

                if (i < passedParams.Count - 1) argData += ", ";
            }
            argData += ")";

            return (returnCommand.Length > 0) ? $"{returnCommand} = {command}{argData}"
                                                                 : $"{command}{argData}";
        }

        public string ToFormatString() => $"{CommandLine} {Comment}";
        public string GetDebuggerDisplay() => ToFormatString();



        private string _comment = string.Empty;
    }
        */
}
