using Parse.MiddleEnd.IR.Datas;
using Parse.Types;

namespace Parse.MiddleEnd.IR.LLVM.Models.VariableModels
{
    public sealed class BitVariableLLVM : VariableLLVM
    {
        public override StdType TypeKind => StdType.Bit;

        public BitVariableLLVM(int offset) : base(offset, 0)
        {
        }

        public BitVariableLLVM(IRDeclareVar var, bool isGlobal) : base(var, isGlobal)
        {
        }
    }
}
