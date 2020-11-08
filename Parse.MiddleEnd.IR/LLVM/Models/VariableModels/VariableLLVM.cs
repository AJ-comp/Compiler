using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using System.Diagnostics;

namespace Parse.MiddleEnd.IR.LLVM.Models.VariableModels
{
    [DebuggerDisplay("{Name}, {Value}, {ValueState}, {PointerLevel}")]
    public abstract class VariableLLVM : DependencyChainVar
    {
        protected VariableLLVM(int offset, uint pointerLevel) : base(pointerLevel)
        {
            IsGlobal = false;
            Offset = offset;
        }

        protected VariableLLVM(string varName, uint pointerLevel) : base(pointerLevel)
        {
            IsGlobal = true;
            _varName = varName;
        }

        protected VariableLLVM(IRVar var, bool isGlobal) : base(var.PointerLevel)
        {
            if (isGlobal)
            {
                _varName = var.Name;
                Name = "@" + _varName;
            }
            else
            {
                Offset = var.Offset;
                Name = "%" + Offset;
            }

            Block = var.Block;
            Length = var.Length;
            IsGlobal = isGlobal;
        }


        public bool IsGlobal
        {
            get => _isGlobal;
            private set
            {
                _isGlobal = value;
                if (_isGlobal) Name = "@" + _varName;
            }
        }
        public override int Offset
        {
            get => _offset;
            set
            {
                _offset = value;
                if (!_isGlobal) Name = "%" + _offset;
            }
        }
        public override int Block { get; set; }
        public override int Length { get; }

        // This property is used to check if any value is assigned.
        public bool New { get; set; } = true;


        public static VariableLLVM From(IRVar var) => From(var, var.TypeName);
        public static VariableLLVM From(IRVar var, int offsetToSet) => From(var, var.TypeName, offsetToSet);


        public static VariableLLVM From(IRVar var, DType toType)
        {
            var result = From(var, toType, true);

            return result;
        }

        public static VariableLLVM From(IRVar var, DType toType, int offsetToSet)
        {
            var result = From(var, toType, false);
            result.Offset = offsetToSet;

            return result;
        }


        private string _varName;
        private bool _isGlobal;
        private int _offset;

        private static VariableLLVM From(IRVar var, DType toType, bool isGlobal)
        {
            VariableLLVM result = null;

            if (toType == DType.Bit) result = new BitVariableLLVM(var, isGlobal);
            else if (toType == DType.Byte) result = new ByteVariableLLVM(var, isGlobal);
            else if (toType == DType.Short) result = new ShortVariableLLVM(var, isGlobal);
            else if (toType == DType.Int) result = new IntVariableLLVM(var, isGlobal);
            else if (toType == DType.Double) result = new DoubleVariableLLVM(var as IRDoubleVar, isGlobal);

            return result;
        }
    }
}
