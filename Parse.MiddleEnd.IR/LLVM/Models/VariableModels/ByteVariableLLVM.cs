using Parse.MiddleEnd.IR.Datas;
using Parse.Types;

namespace Parse.MiddleEnd.IR.LLVM.Models.VariableModels
{
    public class ByteVariableLLVM : IntegerVarLLVM
    {
        public int Size => 8;
        public override StdType TypeKind => StdType.Byte;

        public ByteVariableLLVM(IRDeclareVar var, bool isGlobal) : base(var, isGlobal)
        {
        }

        public ByteVariableLLVM(int offset) : base(offset, 0)
        {
        }

        public ByteVariableLLVM(int offset, uint pointerLevel) : base(offset, pointerLevel)
        {
        }
    }
}
