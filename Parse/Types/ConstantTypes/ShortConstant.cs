namespace Parse.Types.ConstantTypes
{
    public class ShortConstant : IntegerConstant, IShort
    {
        public ShortConstant(short value) : base(value)
        {
        }

        public ShortConstant(ushort value) : base(value)
        {
            Signed = false;
        }

        public ShortConstant(uint pointerLevel, short value) : base(pointerLevel, value)
        {
        }

        public ShortConstant(short value, State valueState, uint pointerLevel) : base(value, valueState, pointerLevel)
        {
        }

        public ShortConstant(ushort value, State valueState, uint pointerLevel) : base(value, valueState, pointerLevel)
        {
        }

        public ShortConstant(ShortConstant t) : base((short)t.Value, t.ValueState, t.PointerLevel)
        {
        }

        public override int Size => 16;
        public override DType TypeName => DType.Short;

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
            else if (to == DType.Short) result = this;
            else if (to == DType.Int)
            {
                result = (Signed) ? new IntConstant((int)Value, ValueState, PointerLevel)
                                          : new IntConstant((uint)Value, ValueState, PointerLevel);
            }
            else if (to == DType.Double)
            {
                result = new DoubleConstant((double)Value, ValueState, PointerLevel);
            }

            return result;
        }
    }
}
