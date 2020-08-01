using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using Parse.Types.ConstantTypes;

namespace Parse.MiddleEnd.IR.LLVM.Models.VariableModels
{
    public class ByteVariableLLVM : IntegerVariableLLVM, IByte
    {
        public override int Size => 8;
        public override DType TypeName => DType.Byte;

        public ByteVariableLLVM(int offset, ByteConstant value) : base(offset, value)
        {
        }

        public ByteVariableLLVM(string varName, ByteConstant value) : base(varName, value)
        {
        }

        public ByteVariableLLVM(int offset, uint pointerLevel) : base(offset, new IntConstant(0, State.NotInit, pointerLevel))
        {
        }

        public ByteVariableLLVM(IRIntegerVar var) : base(var)
        {
        }
    }
}
