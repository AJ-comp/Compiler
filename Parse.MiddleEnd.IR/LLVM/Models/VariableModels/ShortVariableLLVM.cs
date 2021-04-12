using Parse.MiddleEnd.IR.Datas;
using Parse.Types;

namespace Parse.MiddleEnd.IR.LLVM.Models.VariableModels
{
    public class ShortVariableLLVM : IntegerVarLLVM
    {
        public int Size => 16;
        public override StdType TypeKind => StdType.Short;

        public ShortVariableLLVM(IRDeclareVar var, bool isGlobal) : base(var, isGlobal)
        {
        }

        public ShortVariableLLVM(int offset) : base(offset, 0)
        {
        }
    }
}
