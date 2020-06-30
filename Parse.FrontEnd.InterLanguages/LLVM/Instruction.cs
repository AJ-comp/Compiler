using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.LLVM.Models;
using System;

namespace Parse.MiddleEnd.IR.LLVM
{
    public class Instruction : IRUnit
    {
        private string _comment;

        public string CommandLine { get; }
        public SSNode Result { get; }
        public string Comment => _comment;

        public Instruction(string command, string comment = "")
        {
            CommandLine = command;
            _comment = comment;
        }

        public Instruction(string command, SSNode result)
        {
            CommandLine = command;
            Result = result;
        }

        public Instruction(string command, SSNode result, string comment = "") : this(command, comment)
        {
            Result = result;
        }

        internal static Instruction DeclareGlobalVar(string name, DataType dataType, string comment="")
        {
            var type = LLVMConverter.ToInstructionName(dataType);
            int align = LLVMConverter.ToAlign(dataType);

            return new Instruction(string.Format("@{0} = global {1} 0, align {2}", 
                                                                    name, type, align), 
                                                                    comment);
        }

        internal static Instruction DeclareLocalVar(IRVar varData, SSTable ssVarTable, string comment="")
        {
            var newNode = ssVarTable.NewNode(varData);
            var type = LLVMConverter.ToInstructionName(varData.Type);
            int align = LLVMConverter.ToAlign(varData.Type);

            return new Instruction(string.Format("{0} = alloca {1}, align {2}", 
                                                                    newNode.SSF.Name, type, align), 
                                                                    comment);
        }

        // sample
        // <result> = load <ty>, <ty>* <pointer>[, align <alignment>]
        internal static Instruction Load(ISSVar namedItem, SSTable ssVarTable, string comment = "")
        {
            var newNode = ssVarTable.NewNode(namedItem);
            var type = LLVMConverter.ToInstructionName(newNode.SSF.Type);
            int align = LLVMConverter.ToAlign(newNode.SSF.Type);

            return new Instruction(string.Format("{0} = load {1}, {1}* {2}, align {3}", 
                                                                    newNode.SSF.Name, type, namedItem.Name, align),     // string param
                                                                    newNode,                                                                    // IRUnit
                                                                    comment);                                                                   // comment
        }

        // sample
        // store i32 %3, i32* @a, align 4
        internal static Instruction Store(ISSVar fromSSVar, ISSVar toSSVar, string comment = "")
        {
            var fromType = LLVMConverter.ToInstructionName(fromSSVar.Type);
            int fromAlign = LLVMConverter.ToAlign(fromSSVar.Type);

            return new Instruction(string.Format("store = {0} {1}, {0}* {2}, align {3}", 
                                                                    fromType, fromSSVar.Name, toSSVar.Name, fromAlign), // string param
                                                                    comment);                                                                   // comment
        }

        // sample
        // <result> = add nsw <ty> <op1>, <op2>
        internal static Instruction BinOp(LocalVar op1, LocalVar op2, SSTable ssVarTable, IROperation operation)
        {
            var newNode = ssVarTable.NewNode(op1);
            var binOp = LLVMConverter.ToInstructionName(operation);

            if (op1.Type == DataType.Double)
            {
                return new Instruction(string.Format("{0} = f{1} double {2}, {3}", 
                                                                        newNode.SSF.Name, binOp, op1.Name, op2.Name), 
                                                                        newNode);
            }
            else
            {
                var type = LLVMConverter.ToInstructionName(op1.Type);

                return new Instruction(string.Format("{0} = {1} nsw {2} {3}, {4}", 
                                                                        newNode.SSF, binOp, type, op1.Name, op2.Name), 
                                                                        newNode);
            }
        }

        internal static Instruction BinOp(LocalVar op1, IRValue op2, SSTable ssVarTable, IROperation operation)
        {
            var newNode = ssVarTable.NewNode(op1);
            var binOp = LLVMConverter.ToInstructionName(operation);

            if (op1.Type == DataType.Double)
            {
                return new Instruction(string.Format("{0} = f{1} double {2}, {3}", 
                                                                        newNode.SSF.Name, binOp, op1.Name, op2.Value), 
                                                                        newNode);
            }
            else
            {
                var type = LLVMConverter.ToInstructionName(op1.Type);

                return new Instruction(string.Format("{0} = {1} nsw {2} {3}, {4}", 
                                                                        newNode.SSF.Name, binOp, type, op1.Name, op2.Value), 
                                                                        newNode);
            }
        }

        // sample
        // <result> = sext <ty> <op> to i32
        internal static Instruction SExt(LocalVar op, SSTable ssVarTable)
        {
            var newNode = ssVarTable.NewNode(op);
            var typeSize = LLVMConverter.ToAlign(op.Type);

            return (typeSize >= LLVMConverter.ToAlign(DataType.i32)) ? 
                null : new Instruction(string.Format("{0} = sext {1} {2} to i32", 
                                                                        newNode.SSF.Name, op.Type, op.Name), 
                                                                        newNode);
        }

        // <result> = sitofp i32 <op> to double
        // or
        // <result> = uitofp i32 <op> to double
        internal static Instruction IToFp(LocalVar intVar, SSTable ssVarTable)
        {
            var newNode = ssVarTable.NewNode(intVar);

            if(intVar.IsSigned)
                return new Instruction(string.Format("{0} = sitofp i32 {1} to i32", 
                                                                        newNode.SSF.Name, intVar.Name));
            else
                return new Instruction(string.Format("{0} = uitofp i32 {1} to i32", 
                                                                        newNode.SSF.Name, intVar.Name));
        }

        internal static Instruction FpToI()
        {
            throw new Exception();
        }

        // sample
        // <result> = icmp <cond> <ty> <op1>, <op2>
        internal static Instruction Icmp(IRCondition cond, LocalVar op1, LocalVar op2, SSTable ssVarTable)
        {
            if (op1.Type != DataType.i32) return null;
            if (op2.Type != DataType.i32) return null;

            var newNode = ssVarTable.NewNode(false);
            var isSigned = LLVMChecker.IsSigned(op1, op2);
            var condIns = LLVMConverter.ToInstructionNameForInteger(cond, isSigned);

            return new Instruction(string.Format("{0} = icmp {1} {2} {3}, {4}", 
                                                                    newNode.SSF.Name, condIns, op1.Type, op1.Name, op2.Name), 
                                                                    newNode);
        }

        // sample
        // <result> = icmp <cond> <ty> <op1>, <op2>
        internal static Instruction Icmp(IRCondition cond, LocalVar op1, IRIntegerLiteral op2, SSTable ssVarTable)
        {
            if (op1.Type != DataType.i32) return null;

            var newNode = ssVarTable.NewNode(false);
            var isSigned = LLVMChecker.IsSigned(op1, op2);
            var condIns = LLVMConverter.ToInstructionNameForInteger(cond, isSigned);

            return new Instruction(string.Format("{0} = icmp {1} {2} {3}, {4}", 
                                                                    newNode.SSF.Name, condIns, op1.Type, op1.Name, op2.ValueRealType), 
                                                                    newNode);
        }

        // sample
        // <result> = fcmp[fast - math flags]* <cond> <ty> <op1>, <op2>
        internal static Instruction Fcmp(IRCondition cond, LocalVar op1, LocalVar op2, SSTable ssVarTable)
        {
            if (op1.Type != DataType.Double) return null;
            if (op2.Type != DataType.Double) return null;

            var newNode = ssVarTable.NewNode(false);
            var isNan = LLVMChecker.IsNans(op1, op2);
            var condIns = LLVMConverter.ToInstructionNameForDouble(cond, isNan);

            return new Instruction(string.Format("{0} = fcmp {1} {2} {3}, {4}", 
                                                                    newNode.SSF.Name, condIns, op1.Type, op1.Name, op2.Name), 
                                                                    newNode);
        }

        // sample
        // <result> = fcmp[fast - math flags]* <cond> <ty> <op1>, <op2>
        internal static Instruction Fcmp(IRCondition cond, LocalVar op1, IRDoubleLiteral op2, SSTable ssVarTable)
        {
            if (op1.Type != DataType.Double) return null;

            var newNode = ssVarTable.NewNode(false);
            var isNan = LLVMChecker.IsNans(op1, op2);
            var condIns = LLVMConverter.ToInstructionNameForDouble(cond, isNan);

            return new Instruction(string.Format("{0} = fcmp {1} {2} {3}, {4}", 
                                                                    newNode.SSF.Name, condIns, op1.Type, op1.Name, op2.ValueRealType), 
                                                                    newNode);
        }

        // sample
        // br i1 <condlabel>, label <iftrue>, label <iffalse>
        internal static Instruction CBranch(LocalVar cond, LocalVar trueLabel, LocalVar falseLabel)
        {
            if (cond.Type != DataType.i1) return null;
            if (trueLabel.Type != DataType.i1) return null;
            if (falseLabel.Type != DataType.i1) return null;

            return new Instruction(string.Format("br i1 {0}, label {1}, label {2}", 
                                                                    cond.Name, trueLabel.Name, falseLabel.Name));
        }

        // sample
        // br label <dest>
        internal static Instruction UCBranch(LocalVar destLabel)
        {
            return new Instruction(string.Format("br label {0}", destLabel));
        }

        public string ToFormatString() => string.Format("{0} {1}", CommandLine, Comment);

        public override string ToString() => ToFormatString();
    }
}
