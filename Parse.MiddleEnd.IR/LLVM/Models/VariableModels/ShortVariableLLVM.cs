using Parse.MiddleEnd.IR.Datas;
using Parse.Types;

namespace Parse.MiddleEnd.IR.LLVM.Models.VariableModels
{
    public class ShortVariableLLVM : IntegerVarLLVM
    {
        public int Size => 16;
        public override DType TypeName => DType.Short;

        public ShortVariableLLVM(IRVar var, bool isGlobal) : base(var, isGlobal)
        {
        }
    }
}
