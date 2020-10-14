﻿using Parse.Types.Operations;

namespace Parse.Types.ConstantTypes
{
    public class DoubleConstant : Constant, IDouble
    {
        public DoubleConstant(double value) : this(value, State.Fixed)
        {
            Nan = false;
        }

        public DoubleConstant(double value, State valueState) : base(value, valueState)
        {
        }

        public int Size => 64;
        public bool Nan { get; }

        public override DType TypeName => DType.Double;

        public IConstant Add(IValue operand) => Operation.ArithmeticAdd(this, operand);
        public IConstant Div(IValue operand) => Operation.ArithmeticDiv(this, operand);
        public IConstant Equal(IValue operand) => Operation.ArithmeticEqual(this, operand);
        public IConstant Mod(IValue operand) => Operation.ArithmeticMod(this, operand);
        public IConstant Mul(IValue operand) => Operation.ArithmeticMul(this, operand);
        public IConstant NotEqual(IValue operand) => Operation.ArithmeticNotEqual(this, operand);
        public IConstant Sub(IValue operand) => Operation.ArithmeticSub(this, operand);


        public override Constant Casting(DType to)
        {
            Constant result = null;

            if (to == DType.Bit)
            {
                var data = ((int)Value == 0) ? false : true;
                result = new BitConstant(data, ValueState);
            }
            else if (to == DType.Byte) result = new ByteConstant((byte)Value, ValueState);
            else if (to == DType.Short) result = new ShortConstant((short)Value, ValueState);
            else if (to == DType.Int) result = new IntConstant((int)Value, ValueState);
            else if (to == DType.Double) result = this;

            return result;
        }
    }
}
