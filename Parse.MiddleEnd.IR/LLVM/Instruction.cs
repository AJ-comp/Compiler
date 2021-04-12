using Parse.Extensions;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.MiddleEnd.IR.LLVM.Expressions.StmtExpressions;
using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using Parse.Types;
using Parse.Types.ConstantTypes;
using Parse.Types.VarTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parse.MiddleEnd.IR.LLVM
{
    public partial class Instruction : IRUnit
    {
        public string CommandLine { get; }
        public SSAVar NewSSAVar { get; }
        public string Comment => (_comment.Length > 0) ? ";" + _comment : string.Empty;

        public string FullData => CommandLine + Comment;

        public Instruction(string command, string comment = "")
        {
            CommandLine = command;
            _comment = comment;
        }

        public Instruction(string command, SSAVar var, string comment = "")
        {
            CommandLine = command;
            NewSSAVar = var as VariableLLVM;
            _comment = comment;
        }

        // sample format
        // int a;
        // @a = [common] global i32 0, align 4
        internal static Instruction DeclareGlobalVar(RootChainVarContainer var, string comment = "")
        {
            int align = LLVMConverter.ToAlignSize(var.VarInfo);
            var type = LLVMConverter.ToInstructionName(var.VarInfo);

            return new Instruction(string.Format("{0} = global {1} {2}, align {3}",
                                                                    var.LinkedObject.Name, type, var.Name, align),
                                                                    comment);
        }

        internal static Instruction DeclareLocalVar(RootChainVarContainer var, string comment = "")
            => DeclareLocalVar(var.LinkedObject, comment);


        internal static Instruction DeclareLocalVar(SSAVar var, string comment = "")
        {
            int align = LLVMConverter.ToAlignSize(var);
            var type = LLVMConverter.ToInstructionName(var);

            return new Instruction(string.Format("{0} = alloca {1}, align {2}",
                                                                    var.Name, type, align),
                                                                    comment);
        }


        internal static IEnumerable<Instruction> DefineStruct(IRStructDefInfo structData, LLVMSSATable ssaTable, string comment = "")
        {
            var instructionString = string.Format("%struct.{0} = type", structData.Name);

            instructionString += "{";
            instructionString += structData.MemberTypeList.ItemsString(PrintType.String);
            instructionString += "}";

            List<Instruction> result = new List<Instruction>();

            result.Add(new Instruction(instructionString, comment));
            foreach (var funcData in structData.FuncDefList)
                result.AddRange(DefineFunc(funcData, ssaTable, comment));

            return result;
        }

        internal static Instruction DeclareStruct(RootChainVarContainer var, string comment = "")
        {
            int align = LLVMConverter.ToAlignSize(var.VarInfo);

            return new Instruction(string.Format("{0} = alloca {1}, align {2}",
                                                                    var.LinkedObject.Name, var.Name, align),
                                                                    comment);
        }


        internal static IEnumerable<Instruction> DefineFunc(IRFuncDefInfo funcDefInfo,
                                                                                    LLVMSSATable ssaTable,
                                                                                    string comment = "")
        {
            List<Instruction> result = new List<Instruction>();

            // generate param code
            string argumentString = "(";
            foreach (var argument in funcDefInfo.Arguments)
                argumentString += LLVMConverter.ToInstructionName(argument) + ",";

            if (funcDefInfo.Arguments.Count() > 0) argumentString = argumentString[0..^1];
            argumentString += ")";

            result.Add(new Instruction(string.Format("define {0} @{1}{2} #{3}",
                                                                        LLVMConverter.ToInstructionName(funcDefInfo.ReturnType),
                                                                        funcDefInfo.Name,
                                                                        argumentString,
                                                                        funcDefInfo.Index)));

            // generate block code
            result.Add(new Instruction("{"));

            List<RootChainVarContainer> paramValues = new List<RootChainVarContainer>();
            // register value
            foreach (var paramData in funcDefInfo.Arguments)
            {
                //                var param = RootChainVar.Copy(paramData.Var, "!param" + paramData.Var.Offset);
                //                paramValues.Add(ssaTable.RegisterRootChainVarToLocal(param));

                paramValues.Add(ssaTable.RegisterRootChainVarToLocal(paramData));
            }

            // register return variable
            ssaTable.RegisterReturn();

            // register param variable
            foreach (var paramData in funcDefInfo.Arguments)
            {
                var rootChainVar = ssaTable.RegisterRootChainVarToLocal(paramData);
                var newChainVar = ssaTable.NewLink(rootChainVar.LinkedObject);
                result.Add(DeclareLocalVar(newChainVar));
                result.Add(Store(rootChainVar.LinkedObject, newChainVar));
            }

            result.AddRangeExceptNull(LLVMStmtExpression.Create(funcDefInfo.Statement, ssaTable).Build());
            if (funcDefInfo.ReturnType == StdType.Void) result.Add(new Instruction("ret void"));
            result.Add(new Instruction("}"));
            result.Add(EmptyLine());

            return result;
        }


        /*
        internal static Instruction GetElementFromStruct(IRStructDefInfo structDefInfo, )
        {

        }
        */


        // sample
        // <result> = load <ty>, <ty>* <pointer>[, align <alignment>]
        internal static Instruction Load(VariableLLVM useVar, LLVMSSATable ssaTable, string comment = "")
        {
            var newVar = ssaTable.NewLink(useVar);

            int align = LLVMConverter.ToAlignSize(newVar);
            var type = LLVMConverter.ToInstructionName(newVar);

            return new Instruction(string.Format("{0} = load {1}, {1}* {2}, align {3}",
                                                                    newVar.Name, type, useVar.Name, align),     // string param
                                                                    newVar,
                                                                    comment);                                                  // comment
        }


        internal static Instruction Store(ISSAForm from, IRDeclareVar to, bool deRef = false, string comment = "")
        {
            return (from is IRDeclareVar) ? Store(deRef, from as IRDeclareVar, to, comment) : Store(deRef, from as IConstant, to, comment);
        }

        // sample
        // store i32 %3, i32* @a, align 4
        // 모호성 오류를 피하기 위해 파라메터 순서를 바꿉니다.
        private static Instruction Store(bool deRef, IRDeclareVar fromSSVar, IRDeclareVar toSSVar, string comment = "")
        {
            if (deRef)
            {
                if (fromSSVar.PointerLevel != toSSVar.PointerLevel - 1) throw new FormatException();
            }

            int fromAlign = LLVMConverter.ToAlignSize(fromSSVar);
            var fromType = LLVMConverter.ToInstructionName(fromSSVar);

            return new Instruction(string.Format("store {0} {1}, {0}* {2}, align {3}",
                                                                    fromType, fromSSVar.Name, toSSVar.Name, fromAlign), // string param
                                                                    comment);                                                                   // comment
        }

        // sample
        // store i32 10, i32* %1, align 4
        // 모호성 오류를 피하기 위해 파라메터 순서를 바꿉니다.
        private static Instruction Store(bool deRef, IConstant value, IRDeclareVar toSSVar, string comment = "")
        {
            if (deRef)
            {
                if (toSSVar.PointerLevel == 0) throw new FormatException();
                toSSVar.PointerLevel--;
            }

            int fromAlign = LLVMConverter.ToAlignSize(toSSVar);
            var toType = LLVMConverter.ToInstructionName(toSSVar);

            string valueString = (toSSVar.PointerLevel > 0)
                                      ? string.Format("inttoptr (i64 {0} to {1})", value.Value, toType) : value.Value.ToString();

            var result = new Instruction(string.Format("store {0} {1}, {0}* {2}, align {3}",
                                                                    toType, valueString, toSSVar.Name, fromAlign),      // string param
                                                                    comment);                                                               // comment

            if (deRef) toSSVar.PointerLevel++;

            return result;
        }

        // sample
        // <result> = add nsw <ty> <op1>, <op2>
        internal static Instruction BinOp(VariableLLVM op1, VariableLLVM op2, LLVMSSATable ssaTable, IROperation operation)
        {
            var newVar = ssaTable.NewLink(op1, op2);
            var binOp = LLVMConverter.ToInstructionName(operation);

            if (op1 is DoubleVariableLLVM)
            {
                return new Instruction(string.Format("{0} = f{1} double {2}, {3}",
                                                                        newVar.Name, binOp, op1.Name, op2.Name),
                                                                        newVar);
            }
            else
            {
                var type = LLVMConverter.ToInstructionName(op1.TypeKind);

                return new Instruction(string.Format("{0} = {1} nsw {2} {3}, {4}",
                                                                        newVar.Name, binOp, type, op1.Name, op2.Name),
                                                                        newVar);
            }
        }

        internal static Instruction BinOp(VariableLLVM op1, IConstant op2, LLVMSSATable ssTable, IROperation operation)
        {
            var newVar = ssTable.NewLink(op1);
            var binOp = LLVMConverter.ToInstructionName(operation);

            if (op1.TypeKind == StdType.Double)
            {
                return new Instruction(string.Format("{0} = f{1} double {2}, {3}",
                                                                        newVar.Name, binOp, op1.Name, op2.Value),
                                                                        newVar);
            }
            else
            {
                var type = LLVMConverter.ToInstructionName(op1.TypeKind);

                return new Instruction(string.Format("{0} = {1} nsw {2} {3}, {4}",
                                                                        newVar.Name, binOp, type, op1.Name, op2.Value),
                                                                        newVar);
            }
        }

        // sample
        // <result> = sext <ty> <op> to <to_op> || <result> = trunc <ty> <op> to <to_op>
        internal static Instruction ConvertType(VariableLLVM op, StdType toType, LLVMSSATable ssTable)
        {
            var typeSize = LLVMConverter.ToAlignSize(op);
            var toTypeSize = LLVMConverter.ToAlignSize(toType);

            if (toType == StdType.Double) throw new ArgumentException();
            if (typeSize == toTypeSize) throw new ArgumentException();

            var typeName = LLVMConverter.ToInstructionName(op.TypeKind);
            var toTypeName = LLVMConverter.ToInstructionName(toType);

            var newVar = ssTable.NewLink(toType, op);

            if (typeSize < toTypeSize)
            {
                var command = (typeSize == 0) ? "zext" : "sext";

                return new Instruction(string.Format("{0} = {1} {2} {3} to {4}",
                                    newVar.Name, command, typeName, op.Name, toTypeName),
                                    newVar);
            }
            else
            {
                return new Instruction(string.Format("{0} = trunc {1} {2} to {3}",
                                                        newVar.Name, toTypeName, op.Name, typeName),
                                                        newVar);
            }
        }

        // <result> = sitofp i32 <op> to double
        // or
        // <result> = uitofp i32 <op> to double
        internal static Instruction IToFp(IntVariableLLVM var, LLVMSSATable ssTable)
        {
            var newVar = ssTable.NewLink(StdType.Double, var);

            if (var.Signed)
                return new Instruction(string.Format("{0} = sitofp i32 {1} to i32",
                                                                        newVar.Name, var.Name),
                                                                        newVar);
            else
                return new Instruction(string.Format("{0} = uitofp i32 {1} to i32",
                                                                        newVar.Name, var.Name),
                                                                        newVar);
        }

        internal static Instruction FpToI(DoubleVariableLLVM var, LLVMSSATable ssTable)
        {
            throw new Exception();
        }

        // sample (ICMP Abstract)
        // <result> = icmp <cond> <ty> <op1>, <op2>
        internal static Instruction IcmpA(IRCompareSymbol cond, IValue op1, IValue op2, LLVMSSATable ssTable)
        {
            if (op1.TypeKind != StdType.Int) throw new FormatException();
            if (op2.TypeKind != StdType.Int) throw new FormatException();

            return (op1 is IRDeclareVar && op2 is IRDeclareVar) ? Icmp(cond, op1 as IntVariableLLVM, op2 as IntVariableLLVM, ssTable) :
                      (op1 is IRDeclareVar && op2 is IConstant) ? Icmp(cond, op1 as IntVariableLLVM, op2 as IntConstant, ssTable) :
                      (op1 is IConstant && op2 is IRDeclareVar) ? Icmp(cond, op2 as IntVariableLLVM, op1 as IntConstant, ssTable)
                                                                             : throw new FormatException();
        }

        // sample
        // <result> = icmp <cond> <ty> <op1>, <op2>
        internal static Instruction Icmp(IRCompareSymbol cond, IntVariableLLVM op1, IntVariableLLVM op2, LLVMSSATable ssaTable)
        {
            var newSSVar = ssaTable.NewLink(StdType.Bit, op1, op2);
            var isAllSigned = (op1.Signed && op2.Signed);
            var typeName = LLVMConverter.ToInstructionName(op1.TypeKind);
            var condIns = LLVMConverter.GetInstructionNameForInteger(cond, isAllSigned);

            return new Instruction(string.Format("{0} = icmp {1} {2} {3}, {4}",
                                                                    newSSVar.Name, condIns, typeName, op1.Name, op2.Name),
                                                                    newSSVar);
        }

        // sample
        // <result> = icmp <cond> <ty> <op1>, <op2>
        internal static Instruction Icmp(IRCompareSymbol cond, IntVariableLLVM op1, IntConstant op2, LLVMSSATable ssaTable)
        {
            var newVar = ssaTable.NewLink(StdType.Bit, op1);
            var isSigned = (op1.Signed && op2.Signed);
            var typeName = LLVMConverter.ToInstructionName(op1.TypeKind);
            var condIns = LLVMConverter.GetInstructionNameForInteger(cond, isSigned);

            return new Instruction(string.Format("{0} = icmp {1} {2} {3}, {4}",
                                                                    newVar.Name, condIns, typeName, op1.Name, op2.Value),
                                                                    newVar);
        }

        // sample (FCMP Abstract)
        // <result> = fcmp[fast - math flags]* <cond> <ty> <op1>, <op2>
        internal static Instruction FcmpA(IRCompareSymbol cond, IValue op1, IValue op2, LLVMSSATable ssTable)
        {
            if (op1.TypeKind != StdType.Double) throw new FormatException();
            if (op2.TypeKind != StdType.Double) throw new FormatException();

            return (op1 is IVariable && op2 is IVariable) ? Fcmp(cond, op1 as DoubleVariableLLVM, op2 as DoubleVariableLLVM, ssTable) :
                      (op1 is IVariable && op2 is IConstant) ? Fcmp(cond, op1 as DoubleVariableLLVM, op2 as DoubleConstant, ssTable) :
                      (op1 is IConstant && op2 is IVariable) ? Fcmp(cond, op2 as DoubleVariableLLVM, op1 as DoubleConstant, ssTable)
                                                                             : throw new FormatException();
        }

        // sample
        // <result> = fcmp[fast - math flags]* <cond> <ty> <op1>, <op2>
        internal static Instruction Fcmp(IRCompareSymbol cond, DoubleVariableLLVM op1, DoubleVariableLLVM op2, LLVMSSATable ssaTable)
        {
            var newVar = ssaTable.NewLink(StdType.Bit, op1, op2);
            var isNan = (op1.Nan && op2.Nan);
            var typeName = LLVMConverter.ToInstructionName(op1.TypeKind);
            var condIns = LLVMConverter.GetInstructionNameForDouble(cond, isNan);

            return new Instruction(string.Format("{0} = fcmp {1} {2} {3}, {4}",
                                                                    newVar.Name, condIns, typeName, op1.Name, op2.Name),
                                                                    newVar);
        }

        // sample
        // <result> = fcmp[fast - math flags]* <cond> <ty> <op1>, <op2>
        internal static Instruction Fcmp(IRCompareSymbol cond, DoubleVariableLLVM op1, DoubleConstant op2, LLVMSSATable ssaTable)
        {
            var newVar = ssaTable.NewLink(StdType.Bit, op1);
            var isNan = (op1.Nan && op2.Nan);
            var typeName = LLVMConverter.ToInstructionName(op1.TypeKind);
            var condIns = LLVMConverter.GetInstructionNameForDouble(cond, isNan);

            return new Instruction(string.Format("{0} = fcmp {1} {2} {3}, {4}",
                                                                    newVar.Name, condIns, typeName, op1.Name, op2.Value),
                                                                    newVar);
        }

        // sample
        // br i1 <condlabel>, label <iftrue>, label <iffalse>
        internal static Instruction CBranch(BitVariableLLVM cond, BitVariableLLVM trueLabel, BitVariableLLVM falseLabel)
        {
            return new Instruction(string.Format("br i1 {0}, label {1}, label {2}",
                                                                    cond.Name, trueLabel.Name, falseLabel.Name));
        }

        // sample
        // br label <dest>
        internal static Instruction UCBranch(VariableLLVM destLabel) => new Instruction(string.Format("br label {0}", destLabel.Name));

        internal static Instruction GetElement(IRUseMemberVarExpr expr, UserDefVariableLLVM op1, LLVMSSATable ssaTable)
        {
            var newVar = ssaTable.NewLink(op1);
            var typeName = LLVMConverter.ToInstructionName(expr.StructVar);

            return new Instruction(string.Format("{0} = getelementptr inbounds {1}, {1}* {2}, i32 0 i32 {3}",
                                                                    newVar.Name, typeName, op1.Name, expr.Offset));
        }

        internal static Instruction Call(IRFuncDefInfo irFuncData, IReadOnlyList<ISSAForm> passedParams, LLVMSSATable ssaTable)
        {
            string returnCommand = string.Empty;
            if (irFuncData.ReturnType != StdType.Void)
            {
                var resultVar = ssaTable.NewLink(irFuncData.ReturnType);
                returnCommand = resultVar.Name;
            }

            string command = string.Format("call {0} @{1}", LLVMConverter.ToInstructionName(irFuncData.ReturnType), irFuncData.Name);

            string argData = "(";
            for (int i = 0; i < passedParams.Count; i++)
            {
                var passedParam = passedParams[i];

                argData += LLVMConverter.ToInstructionName(passedParam.TypeKind) + " ";

                if (passedParam is IConstant)
                    argData += (passedParam as IConstant).Value;
                else if (passedParam is VariableLLVM)
                    argData += (passedParam as VariableLLVM).Name;
                else throw new FormatException();

                if (i < passedParams.Count - 1) argData += ", ";
            }
            argData += ")";

            return (returnCommand.Length > 0) ? new Instruction(string.Format("{0} = {1}{2}", returnCommand, command, argData))
                                                                 : new Instruction(string.Format("{0}{1}", command, argData));
        }

        internal static Instruction EmptyLine(string comment = "") => new Instruction(string.Empty, comment);

        public string ToFormatString() => string.Format("{0} {1}", CommandLine, Comment);

        public override string ToString() => ToFormatString();




        private string _comment = string.Empty;
    }
}
