using Parse.FrontEnd.InterLanguages.Datas;
using Parse.FrontEnd.InterLanguages.LLVM.Models;
using System;
using System.Linq;

namespace Parse.FrontEnd.InterLanguages.LLVM
{
    public class Instruction : IRUnit
    {
        private string _comment;

        public string CommandLine { get; }

        public string Comment => throw new NotImplementedException();

        public Instruction(string command, string comment = "")
        {
            CommandLine = command;
            _comment = comment;
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
        // %1 = load i32, i32* @a, align 4
        internal static Instruction Load(SSVarData toLinkSSVar, SSVarTable ssVarTable, string comment = "")
        {
            var newSSVar = ssVarTable.CreateNewSSVar(toLinkSSVar);
            var type = LLVMConverter.ToInstructionName(newSSVar.Type);
            int align = LLVMConverter.ToAlign(newSSVar.Type);

            return new Instruction(string.Format("{0} = load {1}, {1}* {2}, align {3}", newSSVar.Name, type, toLinkSSVar.Name, align), comment);
        }

        // sample
        // store i32 %3, i32* @a, align 4
        internal static Instruction Store(SSVarData fromSSVar, SSVarData toSSVar, string comment = "")
        {
            var fromType = LLVMConverter.ToInstructionName(fromSSVar.Type);
            int fromAlign = LLVMConverter.ToAlign(fromSSVar.Type);

            return new Instruction(string.Format("store = {0} {1}, {0}* {2}, align {3}", fromType, fromSSVar.Name, toSSVar.Name, fromAlign), comment);
        }

        internal static Instruction BinOp(SSVarData fromSSVar, SSVarData fromSSVar2, SSVarTable ssVarTable, IROperation operation)
        {
            var newSSVar = ssVarTable.CreateNewSSVar(fromSSVar);
            var binOp = LLVMConverter.ToInstructionName(operation);

            if (fromSSVar.Type == DataType.Double)
            {
                return new Instruction(string.Format("{0} = f{1} double {2}, {3}", newSSVar.Name, binOp, fromSSVar.Name, (double)value));
            }
            else
            {
                var type = LLVMConverter.ToInstructionName(fromSSVar.Type);

                return new Instruction(string.Format("%{0} = {1} nsw {2} {3}, {4}", newSSVar.Name, binOp, type, fromSSVar.Name, (int)value));
            }
        }

        internal static Instruction BinOp(SSVarData fromSSVar, IRLiteralData literal, SSVarTable ssVarTable, IROperation operation)
        {
            var newSSVar = ssVarTable.CreateNewSSVar(fromSSVar);
            var binOp = LLVMConverter.ToInstructionName(operation);

            if (fromSSVar.Type == DataType.Double)
            {
                return new Instruction(string.Format("{0} = f{1} double {2}, {3}", newSSVar.Name, binOp, fromSSVar.Name, literal.Value));
            }
            else
            {
                var type = LLVMConverter.ToInstructionName(fromSSVar.Type);

                return new Instruction(string.Format("%{0} = {1} nsw {2} {3}, {4}", newSSVar.Name, binOp, type, fromSSVar.Name, literal.Value));
            }
        }

        // sample
        // %3 = sext i16 %2 to i32
        internal static Instruction SExt(int newOffset, IRVarData varData, DataType to)
        {
            return (LLVMChecker.IsGreater(varData.Type, to)) ? 
                        new Instruction(string.Format("%{0} = sext {1} {2}", newOffset, varData.Type, to)) :
                        null;
        }

        internal static Instruction FpToSi()
        {
            throw new Exception();
        }

        public string ToFormatString() => string.Format("{0} {1}", CommandLine, Comment);

        public override string ToString() => ToFormatString();
    }
}
