using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions;
using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using Parse.Types;
using Parse.Types.ConstantTypes;
using Parse.Types.VarTypes;
using System;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM
{
    public class Instruction : IRUnit
    {
        private string _comment;

        public string CommandLine { get; }
        public VariableLLVM NewSSAVar { get; }
        public string Comment => (_comment.Length > 0) ?  ";" + _comment : string.Empty;

        public string FullData => CommandLine + Comment;

        public Instruction(string command, string comment = "")
        {
            CommandLine = command;
            _comment = comment;
        }

        public Instruction(string command, DependencyChainVar var, string comment = "")
        {
            CommandLine = command;
            NewSSAVar = var as VariableLLVM;
            _comment = comment;
        }

        // sample format
        // int a;
        // @a = [common] global i32 0, align 4
        internal static Instruction DeclareGlobalVar(RootChainVar var, string comment = "")
        {
            var type = LLVMConverter.ToInstructionName(var.TypeName);
            int align = LLVMConverter.ToAlignSize(var.TypeName);

            return new Instruction(string.Format("{0} = global {1} {2}, align {3}",
                                                                    var.LinkedObject.Name, type, var.Value, align),
                                                                    comment);
        }

        internal static Instruction DeclareLocalVar(RootChainVar var, string comment = "")
        {
            var type = LLVMConverter.ToInstructionName(var.TypeName);
            int align = LLVMConverter.ToAlignSize(var.TypeName);

            return new Instruction(string.Format("{0} = alloca {1}, align {2}",
                                                                    var.LinkedObject.Name, type, align),
                                                                    comment);
        }

        // sample
        // <result> = load <ty>, <ty>* <pointer>[, align <alignment>]
        internal static Instruction Load(VariableLLVM useVar, LLVMSSATable ssTable, string comment = "")
        {
            var newVar = ssTable.NewLink(useVar);
            var type = LLVMConverter.ToInstructionName(newVar.TypeName);
            int align = LLVMConverter.ToAlignSize(newVar.TypeName);

            return new Instruction(string.Format("{0} = load {1}, {1}* {2}, align {3}",
                                                                    newVar.Name, type, useVar.Name, align),     // string param
                                                                    newVar,
                                                                    comment);                                                  // comment
        }

        // sample
        // store i32 %3, i32* @a, align 4
        internal static Instruction Store(IRVar fromSSVar, IRVar toSSVar, string comment = "")
        {
            var fromType = LLVMConverter.ToInstructionName(fromSSVar.TypeName);
            int fromAlign = LLVMConverter.ToAlignSize(fromSSVar.TypeName);

            return new Instruction(string.Format("store {0} {1}, {0}* {2}, align {3}", 
                                                                    fromType, fromSSVar.Name, toSSVar.Name, fromAlign), // string param
                                                                    comment);                                                                   // comment
        }

        // sample
        // store i32 10, i32* %1, align 4
        internal static Instruction Store(IConstant value, IRVar toSSVar, string comment = "")
        {
            var fromType = LLVMConverter.ToInstructionName(toSSVar.TypeName);
            int fromAlign = LLVMConverter.ToAlignSize(toSSVar.TypeName);

            return new Instruction(string.Format("store {0} {1}, {0}* {2}, align {3}",
                                                                    fromType, value.Value, toSSVar.Name, fromAlign),      // string param
                                                                    comment);                                                               // comment
        }

        // sample
        // <result> = add nsw <ty> <op1>, <op2>
        internal static Instruction BinOp(VariableLLVM op1, VariableLLVM op2, LLVMSSATable ssTable, IROperation operation)
        {
            var newVar = ssTable.NewLink(op1, op2);
            var binOp = Helper.GetEnumDescription(operation);

            if (op1 is DoubleVariableLLVM)
            {
                return new Instruction(string.Format("{0} = f{1} double {2}, {3}", 
                                                                        newVar.Name, binOp, op1.Name, op2.Name),
                                                                        newVar);
            }
            else
            {
                var type = LLVMConverter.ToInstructionName(op1.TypeName);

                return new Instruction(string.Format("{0} = {1} nsw {2} {3}, {4}", 
                                                                        newVar.Name, binOp, type, op1.Name, op2.Name),
                                                                        newVar);
            }
        }

        internal static Instruction BinOp(VariableLLVM op1, IConstant op2, LLVMSSATable ssTable, IROperation operation)
        {
            var newVar = ssTable.NewLink(op1);
            var binOp = Helper.GetEnumDescription(operation);

            if (op1.TypeName == DType.Double)
            {
                return new Instruction(string.Format("{0} = f{1} double {2}, {3}", 
                                                                        newVar.Name, binOp, op1.Name, op2.Value),
                                                                        newVar);
            }
            else
            {
                var type = LLVMConverter.ToInstructionName(op1.TypeName);

                return new Instruction(string.Format("{0} = {1} nsw {2} {3}, {4}", 
                                                                        newVar.Name, binOp, type, op1.Name, op2.Value),
                                                                        newVar);
            }
        }

        // sample
        // <result> = sext <ty> <op> to <to_op> || <result> = trunc <ty> <op> to <to_op>
        internal static Instruction ConvertType(VariableLLVM op, DType toType, LLVMSSATable ssTable)
        {
            var typeSize = LLVMConverter.ToAlignSize(op.TypeName);
            var toTypeSize = LLVMConverter.ToAlignSize(toType);

            if (toType == DType.Double) throw new ArgumentException();
            if (typeSize == toTypeSize) throw new ArgumentException();

            var typeName = LLVMConverter.ToInstructionName(op.TypeName);
            var toTypeName = LLVMConverter.ToInstructionName(toType);

            var newVar = ssTable.NewLink(toType, op);

            if(typeSize < toTypeSize)
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
            var newVar = ssTable.NewLink(DType.Double, var);

            if(var.Signed)
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
        internal static Instruction IcmpA(IRCondition cond, IValue op1, IValue op2, LLVMSSATable ssTable)
        {
            if (op1.TypeName != DType.Int) throw new FormatException();
            if (op2.TypeName != DType.Int) throw new FormatException();

            return (op1 is IVariable && op2 is IVariable) ? Icmp(cond, op1 as IntVariableLLVM, op2 as IntVariableLLVM, ssTable) :
                      (op1 is IVariable && op2 is IConstant) ? Icmp(cond, op1 as IntVariableLLVM, op2 as IntConstant, ssTable) :
                      (op1 is IConstant && op2 is IVariable) ? Icmp(cond, op2 as IntVariableLLVM, op1 as IntConstant, ssTable)
                                                                             : throw new FormatException();
        }

        // sample
        // <result> = icmp <cond> <ty> <op1>, <op2>
        internal static Instruction Icmp(IRCondition cond, IntVariableLLVM op1, IntVariableLLVM op2, LLVMSSATable ssaTable)
        {
            var newSSVar = ssaTable.NewLink(DType.Bit, op1, op2);
            var isAllSigned = (op1.Signed && op2.Signed);
            var typeName = LLVMConverter.ToInstructionName(op1.TypeName);
            var condIns = LLVMConverter.GetInstructionNameForInteger(cond, isAllSigned);

            return new Instruction(string.Format("{0} = icmp {1} {2} {3}, {4}",
                                                                    newSSVar.Name, condIns, typeName, op1.Name, op2.Name),
                                                                    newSSVar);
        }

        // sample
        // <result> = icmp <cond> <ty> <op1>, <op2>
        internal static Instruction Icmp(IRCondition cond, IntVariableLLVM op1, IntConstant op2, LLVMSSATable ssaTable)
        {
            var newVar = ssaTable.NewLink(DType.Bit, op1);
            var isSigned = (op1.Signed && op2.Signed);
            var typeName = LLVMConverter.ToInstructionName(op1.TypeName);
            var condIns = LLVMConverter.GetInstructionNameForInteger(cond, isSigned);

            return new Instruction(string.Format("{0} = icmp {1} {2} {3}, {4}",
                                                                    newVar.Name, condIns, typeName, op1.Name, op2.Value),
                                                                    newVar);
        }

        // sample (FCMP Abstract)
        // <result> = fcmp[fast - math flags]* <cond> <ty> <op1>, <op2>
        internal static Instruction FcmpA(IRCondition cond, IValue op1, IValue op2, LLVMSSATable ssTable)
        {
            if (op1.TypeName != DType.Double) throw new FormatException();
            if (op2.TypeName != DType.Double) throw new FormatException();

            return (op1 is IVariable && op2 is IVariable) ? Fcmp(cond, op1 as DoubleVariableLLVM, op2 as DoubleVariableLLVM, ssTable) :
                      (op1 is IVariable && op2 is IConstant) ? Fcmp(cond, op1 as DoubleVariableLLVM, op2 as DoubleConstant, ssTable) :
                      (op1 is IConstant && op2 is IVariable) ? Fcmp(cond, op2 as DoubleVariableLLVM, op1 as DoubleConstant, ssTable)
                                                                             : throw new FormatException();
        }

        // sample
        // <result> = fcmp[fast - math flags]* <cond> <ty> <op1>, <op2>
        internal static Instruction Fcmp(IRCondition cond, DoubleVariableLLVM op1, DoubleVariableLLVM op2, LLVMSSATable ssaTable)
        {
            var newVar = ssaTable.NewLink(DType.Bit, op1, op2);
            var isNan = (op1.Nan && op2.Nan);
            var typeName = LLVMConverter.ToInstructionName(op1.TypeName);
            var condIns = LLVMConverter.GetInstructionNameForDouble(cond, isNan);

            return new Instruction(string.Format("{0} = fcmp {1} {2} {3}, {4}", 
                                                                    newVar.Name, condIns, typeName, op1.Name, op2.Name),
                                                                    newVar);
        }

        // sample
        // <result> = fcmp[fast - math flags]* <cond> <ty> <op1>, <op2>
        internal static Instruction Fcmp(IRCondition cond, DoubleVariableLLVM op1, DoubleConstant op2, LLVMSSATable ssaTable)
        {
            var newVar = ssaTable.NewLink(DType.Bit, op1);
            var isNan = (op1.Nan && op2.Nan);
            var typeName = LLVMConverter.ToInstructionName(op1.TypeName);
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

        internal static Instruction Call(IRFuncData irFuncData, IReadOnlyList<IValue> passedParams, LLVMSSATable ssaTable)
        {
            string returnCommand = string.Empty;
            if (irFuncData.ReturnType != ReturnType.Void)
            {
                var resultVar = ssaTable.NewLink(irFuncData);
                returnCommand = resultVar.Name;
            }
            
            string command = string.Format("call {0} @{1}", LLVMConverter.ToInstructionName(irFuncData.ReturnType), irFuncData.Name);

            string argData = "(";
            for (int i = 0; i < passedParams.Count; i++)
            {
                var passedParam = passedParams[i];

                argData += LLVMConverter.ToInstructionName(passedParam.TypeName) + " ";

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

        internal static Instruction EmptyLine(string comment="") => new Instruction(string.Empty, comment);

        public string ToFormatString() => string.Format("{0} {1}", CommandLine, Comment);

        public override string ToString() => ToFormatString();
    }
}
