using Parse.MiddleEnd.IR.Datas;

namespace Parse.MiddleEnd.IR.LLVM.Models.VariableModels
{
    public abstract class IntegerVarLLVM : VariableLLVM, IRSignableVar
    {
        protected IntegerVarLLVM(IRVar var, bool isGlobal) : base(var, isGlobal)
        {
        }

        public bool Signed { get; }
    }
}
