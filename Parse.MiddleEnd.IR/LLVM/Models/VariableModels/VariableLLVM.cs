using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using Parse.Types.ConstantTypes;
using Parse.Types.VarTypes;
using System.Diagnostics;

namespace Parse.MiddleEnd.IR.LLVM.Models.VariableModels
{
    [DebuggerDisplay("{Name}, {Value}, {ValueState}, {PointerLevel}")]
    public abstract class VariableLLVM : DependencyChainVar
    {
        protected VariableLLVM(int offset, Constant value) : base(value)
        {
            Offset = offset;
            IsGlobal = false;
        }

        protected VariableLLVM(string varName, Constant value) : base(value)
        {
            _varName = varName;
            IsGlobal = true;
        }

        protected VariableLLVM(IRVar var) : base(var)
        {
            Offset = var.Offset;
            Block = var.Block;
            Length = var.Length;
        }


        public bool IsGlobal { get; private set; }
        public override int Offset { get; protected set; }
        public override int Block { get; }
        public override int Length { get; }
        public override string Name => (IsGlobal) ? "@" + _varName : "%" + Offset;

        // This property is used to check if any value is assigned.
        public bool New { get; set; } = true;


        public static VariableLLVM From(IRVar var)
        {
            VariableLLVM result = null;

            if (var.TypeName == DType.Bit) result = new BitVariableLLVM(var.Name, var.ValueConstant as BitConstant);
            else if (var.TypeName == DType.Byte) result = new ByteVariableLLVM(var.Name, var.ValueConstant as ByteConstant);
            else if (var.TypeName == DType.Short) result = new ShortVariableLLVM(var.Name, var.ValueConstant as ShortConstant);
            else if (var.TypeName == DType.Int) result = new IntVariableLLVM(var.Name, var.ValueConstant as IntConstant);
            else if (var.TypeName == DType.Double) result = new DoubleVariableLLVM(var.Name, var.ValueConstant as DoubleConstant);

            result.Offset = 0;

            return result;
        }

        public static VariableLLVM From(int offset, IRVar var)
        {
            VariableLLVM result = From(var);
            result.IsGlobal = false;
            result.Offset = offset;

            return result;
        }


        private string _varName;
    }
}
