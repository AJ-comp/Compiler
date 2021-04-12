using Parse.MiddleEnd.IR.Datas;
using Parse.Types;

namespace Parse.MiddleEnd.IR.LLVM.Models.VariableModels
{
    public class IntVariableLLVM : IntegerVarLLVM
    {
        public int Size => 32;
        public override StdType TypeKind => StdType.Int;

        public IntVariableLLVM(IRDeclareVar var, bool isGlobal) : base(var, isGlobal)
        {
        }

        public IntVariableLLVM(int offset) : base(offset, 0)
        {
        }
    }
}
