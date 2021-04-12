using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using System.Diagnostics;

namespace Parse.MiddleEnd.IR.LLVM.Models.VariableModels
{
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public abstract class VariableLLVM : SSAVar
    {
        public object Value { get; }
        public State ValueState { get; }

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

        // This property is used to check if any value is assigned.
        public bool New { get; set; } = true;

        protected VariableLLVM(int offset, uint pointerLevel)
        {
            IsGlobal = false;
            Offset = offset;
        }

        protected VariableLLVM(string varName, uint pointerLevel)
        {
            IsGlobal = true;
            _varName = varName;
        }

        protected VariableLLVM(IRDeclareVar var, bool isGlobal)
        {
            InitialExpr = var.InitialExpr;

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
            PointerLevel = var.PointerLevel;
        }


        public static VariableLLVM From(IRDeclareVar var) => From(var, var.TypeKind);
        public static VariableLLVM From(IRDeclareVar var, int offsetToSet) => From(var, var.TypeKind, offsetToSet);


        public static VariableLLVM From(IRDeclareVar var, StdType toType)
        {
            var result = From(var, toType, true);

            return result;
        }

        public static VariableLLVM From(IRDeclareVar var, StdType toType, int offsetToSet)
        {
            var result = From(var, toType, false);
            result.Offset = offsetToSet;

            return result;
        }

        public static VariableLLVM From(int offset, StdType toType)
        {
            VariableLLVM result = null;

            if (toType == StdType.Bit) result = new BitVariableLLVM(offset);
            else if (toType == StdType.Byte) result = new ByteVariableLLVM(offset);
            else if (toType == StdType.Short) result = new ShortVariableLLVM(offset);
            else if (toType == StdType.Int) result = new IntVariableLLVM(offset);
            else if (toType == StdType.Double) result = new DoubleVariableLLVM(offset);
            else if (toType == StdType.Struct) result = new UserDefVariableLLVM(offset, 0);

            return result;
        }


        private string _varName;
        private bool _isGlobal;
        private int _offset;

        private static VariableLLVM From(IRDeclareVar var, StdType toType, bool isGlobal)
        {
            VariableLLVM result = null;

            if (toType == StdType.Bit) result = new BitVariableLLVM(var, isGlobal);
            else if (toType == StdType.Byte) result = new ByteVariableLLVM(var, isGlobal);
            else if (toType == StdType.Short) result = new ShortVariableLLVM(var, isGlobal);
            else if (toType == StdType.Int) result = new IntVariableLLVM(var, isGlobal);
            else if (toType == StdType.Double) result = new DoubleVariableLLVM(var as IRDoubleVar, isGlobal);
            else if (toType == StdType.Struct) result = new UserDefVariableLLVM(var as IRDeclareStructTypeVar, isGlobal);

            return result;
        }

        public override string DebuggerDisplay => base.DebuggerDisplay;
    }
}
