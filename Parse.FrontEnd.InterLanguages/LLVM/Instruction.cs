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
            var type = LLVMConverter.ToString(dataType);
            int align = LLVMConverter.ToAlign(dataType);

            return new Instruction(string.Format("@{0} = global {1} 0, align {2}", name, type, align), comment);
        }

        internal static Instruction DeclareLocalVar(VarData varData, SSVarTable ssVarTable, string comment="")
        {
            var ssVar = ssVarTable.CreateToLocalTable(varData);
            var type = LLVMConverter.ToString(varData.Type);
            int align = LLVMConverter.ToAlign(varData.Type);

            return new Instruction(string.Format("{0} = alloca {1}, align {2}", ssVar.Name, type, align), comment);
        }

        // sample
        // %1 = load i32, i32* @a, align 4
        internal static Instruction Load(SSVarData toLinkSSVar, SSVarTable ssVarTable, string comment = "")
        {
            var newSSVar = ssVarTable.CreateNewSSVar(toLinkSSVar);
            var type = LLVMConverter.ToString(newSSVar.Type);
            int align = LLVMConverter.ToAlign(newSSVar.Type);

            return new Instruction(string.Format("{0} = load {1}, {1}* {2}, align {3}", newSSVar.Name, type, toLinkSSVar.Name, align), comment);
        }

        // sample
        // store i32 %3, i32* @a, align 4
        internal static Instruction Store(SSVarData fromSSVar, SSVarData toSSVar, string comment = "")
        {
            var fromType = LLVMConverter.ToString(fromSSVar.Type);
            int fromAlign = LLVMConverter.ToAlign(fromSSVar.Type);

            return new Instruction(string.Format("store = {0} {1}, {0}* {2}, align {3}", fromType, fromSSVar.Name, toSSVar.Name, fromAlign), comment);
        }

        // sample
        // %2 = add nsw i32 %1, 1
        internal static Instruction Add(SSVarData fromSSVar, SSVarTable ssVarTable, object value)
        {
            var newSSVar = ssVarTable.CreateNewSSVar(fromSSVar);

            if(fromSSVar.Type == DataType.Double)
            {
                return new Instruction(string.Format("{0} = fadd double {1}, {2}", newSSVar.Name, fromSSVar.Name, (double)value));
            }
            else
            {
                var type = LLVMConverter.ToString(fromSSVar.Type);

                return new Instruction(string.Format("%{0} = add nsw {1} {2}, {3}", newSSVar.Name, type, fromSSVar.Name, (int)value));
            }
        }

        // sample
        // %3 = sext i16 %2 to i32
        internal static Instruction SExt(int newOffset, VarData varData, DataType to)
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
