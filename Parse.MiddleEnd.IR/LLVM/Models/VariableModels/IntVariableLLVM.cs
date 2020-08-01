using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using Parse.Types.ConstantTypes;

namespace Parse.MiddleEnd.IR.LLVM.Models.VariableModels
{
    public class IntVariableLLVM : IntegerVariableLLVM, IRIntegerVar
    {
        public override int Size => 32;
        public override DType TypeName => DType.Int;

        public IntVariableLLVM(int offset, IntConstant value) : base(offset, value)
        {
        }
        public IntVariableLLVM(int offset, uint pointerLevel) : base(offset, new IntConstant(0, State.NotInit, pointerLevel))
        {
        }
        public IntVariableLLVM(string name, IntConstant value) : base(name, value)
        {
        }

        public IntVariableLLVM(IRIntegerVar var) : base(var)
        {
        }
    }
}
