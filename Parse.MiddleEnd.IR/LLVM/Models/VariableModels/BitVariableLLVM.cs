using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using Parse.Types.ConstantTypes;

namespace Parse.MiddleEnd.IR.LLVM.Models.VariableModels
{
    public class BitVariableLLVM : VariableLLVM
    {
        public override DType TypeName => DType.Bit;

        public BitVariableLLVM(int offset) : base(offset, 0)
        {
        }

        public BitVariableLLVM(string varName) : base(varName, 0)
        {
        }

        public BitVariableLLVM(IRVar var, bool isGlobal) : base(var, isGlobal)
        {
        }
    }
}
