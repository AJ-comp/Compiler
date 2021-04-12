using Parse.MiddleEnd.IR.Datas;
using Parse.Types;

namespace Parse.MiddleEnd.IR.LLVM.Models.VariableModels
{
    public class DoubleVariableLLVM : VariableLLVM
    {
        public int Size => 64;
        public bool Nan { get; }

        public override StdType TypeKind => StdType.Double;

        public DoubleVariableLLVM(IRDoubleVar var, bool isGlobal) : base(var, isGlobal)
        {
            Nan = var.Nan;
        }

        public DoubleVariableLLVM(int offset) : base(offset, 0)
        {
        }
    }
}
