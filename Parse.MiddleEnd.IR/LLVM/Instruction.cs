using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Datas.Types;
using Parse.MiddleEnd.IR.Datas.ValueDatas;
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

        internal static Instruction DeclareGlobalVar(IRVar irVar, string comment="")
        {
            var type = LLVMConverter.ToInstructionName(irVar.TypeName);
            int align = LLVMConverter.ToSize(irVar.TypeName);

            return new Instruction(string.Format("@{0} = global {1} 0, align {2}", 
                                                                    irVar.Name, type, align), 
                                                                    comment);
        }

        internal static Instruction DeclareLocalVar(IRVar irVar, SSTable ssVarTable, string comment="")
        {
            var newNode = ssVarTable.NewNode(irVar);
            var type = LLVMConverter.ToInstructionName(irVar.TypeName);
            int align = LLVMConverter.ToSize(irVar.TypeName);

            return new Instruction(string.Format("{0} = alloca {1}, align {2}", 
                                                                    newNode.SSF.Name, type, align), 
                                                                    newNode,
                                                                    comment);
        }

        // sample
        // <result> = load <ty>, <ty>* <pointer>[, align <alignment>]
        internal static Instruction Load(IRVar namedItem, SSTable ssVarTable, string comment = "")
        {
            var newNode = ssVarTable.NewNode(namedItem);
            var type = LLVMConverter.ToInstructionName(newNode.SSF.TypeName);
            int align = LLVMConverter.ToSize(newNode.SSF.TypeName);

            return new Instruction(string.Format("{0} = load {1}, {1}* {2}, align {3}", 
                                                                    newNode.SSF.Name, type, namedItem.Name, align),     // string param
                                                                    newNode,                                                                    // IRUnit
                                                                    comment);                                                                   // comment
        }

        // sample
        // store i32 %3, i32* @a, align 4
        internal static Instruction Store(IRVar fromSSVar, IRVar toSSVar, string comment = "")
        {
            var fromType = LLVMConverter.ToInstructionName(fromSSVar.TypeName);
            int fromAlign = LLVMConverter.ToSize(fromSSVar.TypeName);

            return new Instruction(string.Format("store = {0} {1}, {0}* {2}, align {3}", 
                                                                    fromType, fromSSVar.Name, toSSVar.Name, fromAlign), // string param
                                                                    comment);                                                                   // comment
        }

        // sample
        // store i32 10, i32* %1, align 4
        internal static Instruction Store(IRValue value, IRVar toSSVar, string comment = "")
        {
            var fromType = LLVMConverter.ToInstructionName(toSSVar.TypeName);
            int fromAlign = LLVMConverter.ToSize(toSSVar.TypeName);

            return new Instruction(string.Format("store = {0} {1}, {0}* {2}, align {3}",
                                                                    fromType, value.Value, toSSVar.Name, fromAlign), // string param
                                                                    comment);                                                           // comment
        }

        // sample
        // <result> = add nsw <ty> <op1>, <op2>
        internal static Instruction BinOp(LocalVar op1, LocalVar op2, SSTable ssVarTable, IROperation operation)
        {
            var newNode = ssVarTable.NewNode(op1);
            var binOp = LLVMConverter.ToInstructionName(operation);

            if (op1.TypeName == DType.Double)
            {
                return new Instruction(string.Format("{0} = f{1} double {2}, {3}", 
                                                                        newNode.SSF.Name, binOp, op1.Name, op2.Name), 
                                                                        newNode);
            }
            else
            {
                var type = LLVMConverter.ToInstructionName(op1.TypeName);

                return new Instruction(string.Format("{0} = {1} nsw {2} {3}, {4}", 
                                                                        newNode.SSF, binOp, type, op1.Name, op2.Name), 
                                                                        newNode);
            }
        }

        internal static Instruction BinOp(LocalVar op1, SSValue op2, SSTable ssVarTable, IROperation operation)
        {
            var newNode = ssVarTable.NewNode(op1);
            var binOp = LLVMConverter.ToInstructionName(operation);

            if (op1.TypeName == DType.Double)
            {
                return new Instruction(string.Format("{0} = f{1} double {2}, {3}", 
                                                                        newNode.SSF.Name, binOp, op1.Name, op2.Value), 
                                                                        newNode);
            }
            else
            {
                var type = LLVMConverter.ToInstructionName(op1.TypeName);

                return new Instruction(string.Format("{0} = {1} nsw {2} {3}, {4}", 
                                                                        newNode.SSF.Name, binOp, type, op1.Name, op2.Value), 
                                                                        newNode);
            }
        }

        // sample
        // <result> = sext <ty> <op> to i32
        internal static Instruction SExt(LocalVar<Integer> op, SSTable ssVarTable)
        {
            var newNode = ssVarTable.NewNode(op);
            var typeSize = LLVMConverter.ToSize(op.TypeName);

            var toCompare = new LocalVar<Int>(0xff);
            var size = LLVMConverter.ToSize(toCompare.TypeName);
            return (typeSize >= size) ? null : 
                                                    new Instruction(string.Format("{0} = sext {1} {2} to i32", 
                                                                                                    newNode.SSF.Name, op.TypeName, op.Name), 
                                                                                                    newNode);
        }

        // <result> = sitofp i32 <op> to double
        // or
        // <result> = uitofp i32 <op> to double
        internal static Instruction IToFp(LocalVar<Integer> var, SSTable ssVarTable)
        {
            var newNode = ssVarTable.NewNode(var);

            if(var.Signed)
                return new Instruction(string.Format("{0} = sitofp i32 {1} to i32", 
                                                                        newNode.SSF.Name, var.Name));
            else
                return new Instruction(string.Format("{0} = uitofp i32 {1} to i32", 
                                                                        newNode.SSF.Name, var.Name));
        }

        internal static Instruction FpToI()
        {
            throw new Exception();
        }

        // sample
        // <result> = icmp <cond> <ty> <op1>, <op2>
        internal static Instruction Icmp(IRCondition cond, LocalVar<Int> op1, LocalVar<Int> op2, SSTable ssVarTable)
        {
            var newSSVar = ssVarTable.NewNode(false);
            var isAllSigned = (op1.Signed && op2.Signed);
            var condIns = LLVMConverter.GetInstructionNameForInteger(cond, isAllSigned);

            return new Instruction(string.Format("{0} = icmp {1} {2} {3}, {4}", 
                                                                    newSSVar.SSF.Name, condIns, op1.TypeName, op1.Name, op2.Name), 
                                                                    newSSVar);
        }

        // sample
        // <result> = icmp <cond> <ty> <op1>, <op2>
        internal static Instruction Icmp(IRCondition cond, LocalVar<Int> op1, SSValue<Int> op2, SSTable ssVarTable)
        {
            var newNode = ssVarTable.NewNode(false);
            var isSigned = (op1.Signed && op2.Signed);
            var condIns = LLVMConverter.GetInstructionNameForInteger(cond, isSigned);

            return new Instruction(string.Format("{0} = icmp {1} {2} {3}, {4}", 
                                                                    newNode.SSF.Name, condIns, op1.TypeName, op1.Name, op2.Value), 
                                                                    newNode);
        }

        // sample
        // <result> = fcmp[fast - math flags]* <cond> <ty> <op1>, <op2>
        internal static Instruction Fcmp(IRCondition cond, LocalVar<DoubleType> op1, LocalVar<DoubleType> op2, SSTable ssVarTable)
        {
            var newNode = ssVarTable.NewNode(false);
            var isNan = (op1.IsNan && op2.IsNan);
            var condIns = LLVMConverter.GetInstructionNameForDouble(cond, isNan);

            return new Instruction(string.Format("{0} = fcmp {1} {2} {3}, {4}", 
                                                                    newNode.SSF.Name, condIns, op1.TypeName, op1.Name, op2.Name), 
                                                                    newNode);
        }

        // sample
        // <result> = fcmp[fast - math flags]* <cond> <ty> <op1>, <op2>
        internal static Instruction Fcmp(IRCondition cond, LocalVar<DoubleType> op1, SSValue<DoubleType> op2, SSTable ssVarTable)
        {
            var newNode = ssVarTable.NewNode(false);
            var isNan = (op1.IsNan && op2.IsNan);
            var condIns = LLVMConverter.GetInstructionNameForDouble(cond, isNan);

            return new Instruction(string.Format("{0} = fcmp {1} {2} {3}, {4}", 
                                                                    newNode.SSF.Name, condIns, op1.TypeName, op1.Name, op2.Value), 
                                                                    newNode);
        }

        // sample
        // br i1 <condlabel>, label <iftrue>, label <iffalse>
        internal static Instruction CBranch(LocalVar<Bit> cond, LocalVar<Bit> trueLabel, LocalVar<Bit> falseLabel)
        {
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
