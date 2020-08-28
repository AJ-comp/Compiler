namespace Parse.Types.ConstantTypes
{
    public class IntConstant : IntegerConstant, IInt
    {
        public int RealValue => (int)Value;
        public override int Size => 32;
        public override DType TypeName => DType.Int;

        public IntConstant(int value) : base(value)
        {
        }

        public IntConstant(uint value) : base(value)
        {
        }

        public IntConstant(uint pointerLevel, int value) : base(pointerLevel, value)
        {
        }

        public IntConstant(int value, State valueState, uint pointerLevel) : base(value, valueState, pointerLevel)
        {
        }

        public IntConstant(uint value, State valueState, uint pointerLevel) : base(value, valueState, pointerLevel)
        {
        }

        public IntConstant(IntConstant t) : base((int)t.Value, t.ValueState, t.PointerLevel)
        {
        }

        public override Constant Casting(DType to)
        {
            Constant result = null;

            if (to == DType.Bit)
            {
                var data = ((int)Value == 0) ? false : true;
                result = new BitConstant(data, ValueState, PointerLevel);
            }
            else if (to == DType.Byte)
            {
                result = (Signed) ? new ByteConstant((sbyte)Value, ValueState, PointerLevel)
                                          : new ByteConstant((byte)Value, ValueState, PointerLevel);
            }
            else if (to == DType.Short)
            {
                result = (Signed) ? new ShortConstant((short)Value, ValueState, PointerLevel)
                                          : new ShortConstant((ushort)Value, ValueState, PointerLevel);
            }
            else if (to == DType.Int) result = this;
            else if (to == DType.Double)
            {
                result = new DoubleConstant((double)Value, ValueState, PointerLevel);
            }

            return result;
        }
    }
}
