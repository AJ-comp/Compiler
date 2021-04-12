using Parse.MiddleEnd.IR.Datas;

namespace Parse.MiddleEnd.IR.LLVM.Models.VariableModels
{
    public abstract class IntegerVarLLVM : VariableLLVM, IRSignableVar
    {
        protected IntegerVarLLVM(IRDeclareVar var, bool isGlobal) : base(var, isGlobal)
        {
        }

        protected IntegerVarLLVM(int offset, uint pointerLevel) : base(offset, pointerLevel)
        {
        }

        public bool Signed { get; }
    }
}
