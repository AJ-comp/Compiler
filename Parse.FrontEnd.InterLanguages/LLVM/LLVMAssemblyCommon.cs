using Parse.FrontEnd.InterLanguages.LLVM.Models;
using System.Linq;

namespace Parse.FrontEnd.InterLanguages.LLVM
{
    public partial class LLVMAssemblyBuilder : IRBuilder
    {
        private IRBlock LoadAndExp(SSVarData target, bool isItoFpCond)
        {
            IRBlock result = new IRBlock();

            // load
            result.Add(Instruction.Load(target, _ssVarTable));

            // sext (if a condition is met)
            result.Add(Instruction.SExt((result.Last() as Instruction).Result, _ssVarTable));
            if (isItoFpCond)
                result.Add(Instruction.IToFp((result.Last() as Instruction).Result as LocalIntSSVar, _ssVarTable)); // sitofp or uitofp

            return result;
        }
    }
}
