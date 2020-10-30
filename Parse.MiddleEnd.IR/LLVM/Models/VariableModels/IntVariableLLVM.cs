using Parse.MiddleEnd.IR.Datas;
using Parse.Types;

namespace Parse.MiddleEnd.IR.LLVM.Models.VariableModels
{
    public class IntVariableLLVM : IntegerVarLLVM
    {
        public int Size => 32;
        public override DType TypeName => DType.Int;

        public IntVariableLLVM(IRVar var, bool isGlobal) : base(var, isGlobal)
        {
        }
    }
}
