﻿using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using Parse.Types.ConstantTypes;
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


        public static VariableLLVM From(IRVar var) => From(var, var.TypeName);

        public static VariableLLVM From(IRVar var, DType toType)
        {
            VariableLLVM result = null;

            if(var.ValueConstant != null)
            {
                IConstant castingConstant = var.ValueConstant;
                if (castingConstant.TypeName != toType)
                    castingConstant = var.ValueConstant.Casting(toType);

                if (castingConstant.TypeName == DType.Bit) result = new BitVariableLLVM(var.Name, castingConstant as BitConstant);
                else if (castingConstant.TypeName == DType.Byte) result = new ByteVariableLLVM(var.Name, castingConstant as ByteConstant);
                else if (castingConstant.TypeName == DType.Short) result = new ShortVariableLLVM(var.Name, castingConstant as ShortConstant);
                else if (castingConstant.TypeName == DType.Int) result = new IntVariableLLVM(var.Name, castingConstant as IntConstant);
                else if (castingConstant.TypeName == DType.Double) result = new DoubleVariableLLVM(var.Name, castingConstant as DoubleConstant);
            }
            else
            {
                if (toType == DType.Bit) result = new BitVariableLLVM(var.Name, null);
                else if (toType == DType.Byte) result = new ByteVariableLLVM(var.Name, null);
                else if (toType == DType.Short) result = new ShortVariableLLVM(var.Name, null);
                else if (toType == DType.Int) result = new IntVariableLLVM(var.Name, null);
                else if (toType == DType.Double) result = new DoubleVariableLLVM(var.Name, null);
            }

            result.Offset = 0;

            return result;
        }

        public static VariableLLVM From(int offset, IRVar var) => From(offset, var, var.TypeName);

        public static VariableLLVM From(int offset, IRVar var, DType toType)
        {
            VariableLLVM result = From(var, toType);
            result.IsGlobal = false;
            result.Offset = offset;

            return result;
        }


        private string _varName;
    }
}
