using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using Parse.Types.ConstantTypes;

namespace Parse.MiddleEnd.IR.LLVM.Models.VariableModels
{
    public class ShortVariableLLVM : IntegerVariableLLVM, IShort
    {
        public override int Size => 16;
        public override DType TypeName => DType.Short;

        public ShortVariableLLVM(int offset, ShortConstant value) : base(offset, value)
        {
        }

        public ShortVariableLLVM(string varName, ShortConstant value) : base(varName, value)
        {
        }

        public ShortVariableLLVM(int offset, uint pointerLevel) : base(offset, new IntConstant(0, State.NotInit, pointerLevel))
        {
        }

        public ShortVariableLLVM(IRIntegerVar var) : base(var)
        {
        }
    }
}
