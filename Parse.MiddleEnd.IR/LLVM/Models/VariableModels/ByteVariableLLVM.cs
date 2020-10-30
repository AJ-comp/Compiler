using Parse.MiddleEnd.IR.Datas;
using Parse.Types;

namespace Parse.MiddleEnd.IR.LLVM.Models.VariableModels
{
    public class ByteVariableLLVM : IntegerVarLLVM
    {
        public int Size => 8;
        public override DType TypeName => DType.Byte;

        public ByteVariableLLVM(IRVar var, bool isGlobal) : base(var, isGlobal)
        {
        }
    }
}
