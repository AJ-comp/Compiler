using Parse.FrontEnd.InterLanguages.Datas;
using Parse.FrontEnd.InterLanguages.LLVM.Models;
using System;

namespace Parse.FrontEnd.InterLanguages.LLVM
{
    public class Instruction : IRUnit
    {
        private string _comment;

        public string CommandLine { get; }
        public LocalSSVar Result { get; }
        public string Comment => _comment;

        public Instruction(string command, string comment = "")
        {
            CommandLine = command;
            _comment = comment;
        }

        public Instruction(string command, LocalSSVar result)
        {
            CommandLine = command;
            Result = result;
        }

        public Instruction(string command, LocalSSVar result, string comment = "") : this(command, comment)
        {
            Result = result;
        }

        internal static Instruction DeclareGlobalVar(string name, DataType dataType, string comment="")
        {
            var type = LLVMConverter.ToInstructionName(dataType);
            int align = LLVMConverter.ToAlign(dataType);

            return new Instruction(string.Format("@{0} = global {1} 0, align {2}", name, type, align), comment);
        }

        internal static Instruction DeclareLocalVar(IRVarData varData, SSVarTable ssVarTable, string comment="")
        {
            var ssVar = ssVarTable.CreateToLocalTable(varData);
            var type = LLVMConverter.ToInstructionName(varData.Type);
            int align = LLVMConverter.ToAlign(varData.Type);

            return new Instruction(string.Format("{0} = alloca {1}, align {2}", ssVar.Name, type, align), comment);
        }

        // sample
        // <result> = load <ty>, <ty>* <pointer>[, align <alignment>]
        internal static Instruction Load(SSVarData toLinkSSVar, SSVarTable ssVarTable, string comment = "")
        {
            var newSSVar = ssVarTable.CreateNewSSVar(toLinkSSVar);
            var type = LLVMConverter.ToInstructionName(newSSVar.Type);
            int align = LLVMConverter.ToAlign(newSSVar.Type);

            return new Instruction(string.Format("{0} = load {1}, {1}* {2}, align {3}", newSSVar.Name, type, toLinkSSVar.Name, align), 
                                            newSSVar, comment);
        }

        // sample
        // store i32 %3, i32* @a, align 4
        internal static Instruction Store(SSVarData fromSSVar, SSVarData toSSVar, string comment = "")
        {
            var fromType = LLVMConverter.ToInstructionName(fromSSVar.Type);
            int fromAlign = LLVMConverter.ToAlign(fromSSVar.Type);

            return new Instruction(string.Format("store = {0} {1}, {0}* {2}, align {3}", fromType, fromSSVar.Name, toSSVar.Name, fromAlign), comment);
        }

        // sample
        // <result> = add nsw <ty> <op1>, <op2>
        internal static Instruction BinOp(SSVarData op1, SSVarData op2, SSVarTable ssVarTable, IROperation operation)
        {
            var newSSVar = ssVarTable.CreateNewSSVar(op1);
            var binOp = LLVMConverter.ToInstructionName(operation);

            if (op1.Type == DataType.Double)
            {
                return new Instruction(string.Format("{0} = f{1} double {2}, {3}", newSSVar.Name, binOp, op1.Name, op2.Name), newSSVar);
            }
            else
            {
                var type = LLVMConverter.ToInstructionName(op1.Type);

                return new Instruction(string.Format("{0} = {1} nsw {2} {3}, {4}", newSSVar.Name, binOp, type, op1.Name, op2.Name), newSSVar);
            }
        }

        internal static Instruction BinOp(SSVarData fromSSVar, IRLiteralData literal, SSVarTable ssVarTable, IROperation operation)
        {
            var newSSVar = ssVarTable.CreateNewSSVar(fromSSVar);
            var binOp = LLVMConverter.ToInstructionName(operation);

            if (fromSSVar.Type == DataType.Double)
            {
                return new Instruction(string.Format("{0} = f{1} double {2}, {3}", newSSVar.Name, binOp, fromSSVar.Name, literal.Value), newSSVar);
            }
            else
            {
                var type = LLVMConverter.ToInstructionName(fromSSVar.Type);

                return new Instruction(string.Format("{0} = {1} nsw {2} {3}, {4}", newSSVar.Name, binOp, type, fromSSVar.Name, literal.Value), newSSVar);
            }
        }

        // sample
        // <result> = sext <ty> <op> to i32
        internal static Instruction SExt(LocalSSVar varData, SSVarTable ssVarTable)
        {
            var newSSVar = ssVarTable.CreateNewSSVar(varData);
            var typeSize = LLVMConverter.ToAlign(varData.Type);

            return (typeSize >= LLVMConverter.ToAlign(DataType.i32)) ? 
                null : new Instruction(string.Format("{0} = sext {1} {2} to i32", newSSVar.Name, varData.Type, varData.Name), newSSVar);
        }

        // <result> = sitofp i32 <op> to double
        // or
        // <result> = uitofp i32 <op> to double
        internal static Instruction IToFp(LocalIntSSVar intVar, SSVarTable ssVarTable)
        {
            var newSSVar = ssVarTable.CreateNewSSVar(intVar);

            if(intVar.IsUnsigned)
                return new Instruction(string.Format("{0} = uitofp i32 {1} to i32", newSSVar.Name, intVar.Name));
            else
                return new Instruction(string.Format("{0} = sitofp i32 {1} to i32", newSSVar.Name, intVar.Name));
        }

        internal static Instruction FpToI()
        {
            throw new Exception();
        }

        // sample
        // <result> = icmp <cond> <ty> <op1>, <op2>
        internal static Instruction Icmp(IRCondition cond, LocalIntSSVar op1, LocalIntSSVar op2, SSVarTable ssVarTable)
        {
            var newSSVar = ssVarTable.CreateNewSSVar();
            var condIns = LLVMConverter.ToInstructionName(cond);

            return new Instruction(string.Format("{0} = icmp {1} {2} {3}, {4}", newSSVar.Name, condIns, op1.Type, op1.Name, op2.Name), newSSVar);
        }

        // sample
        // <result> = fcmp[fast - math flags]* <cond> <ty> <op1>, <op2>
        internal static Instruction Fcmp(IRCondition cond, LocalDoubleSSVar op1, LocalDoubleSSVar op2, SSVarTable ssVarTable)
        {
            var newSSVar = ssVarTable.CreateNewSSVar();
            var condIns = LLVMConverter.ToInstructionName(cond);

            return new Instruction(string.Format("{0} = fcmp {1} {2} {3}, {4}", newSSVar.Name, condIns, op1.Type, op1.Name, op2.Name), newSSVar);
        }

        // sample
        // br i1 <cond>, label <iftrue>, label <iffalse>
        internal static Instruction CBranch(IRCondition cond, LocalSSVar trueLabel, LocalSSVar falseLabel)
        {
            var condIns = LLVMConverter.ToInstructionName(cond);

            return new Instruction(string.Format("br i1 {0}, label {1}, label {2}", condIns, trueLabel.Name, falseLabel.Name));
        }

        // sample
        // br label <dest>
        internal static Instruction UCBranch(LocalSSVar destLabel)
        {
            return new Instruction(string.Format("br label {0}", destLabel));
        }

        public string ToFormatString() => string.Format("{0} {1}", CommandLine, Comment);

        public override string ToString() => ToFormatString();
    }
}
