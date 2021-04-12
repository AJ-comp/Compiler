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

        public ShortConstant(short value, State valueState) : base(value, valueState)
        {
        }

        public ShortConstant(ushort value, State valueState) : base(value, valueState)
        {
        }

        public ShortConstant(ShortConstant t) : base((short)t.Value, t.ValueState)
        {
        }

        public override int Size => 16;
        public override StdType TypeKind => StdType.Short;

        public override Constant Casting(StdType to)
        {
            Constant result = null;

            if (to == StdType.Bit)
            {
                var data = (int)Value != 0;
                result = new BitConstant(data, ValueState);
            }
            else if (to == StdType.Byte)
            {
                result = (Signed) ? new ByteConstant((sbyte)Value, ValueState)
                                          : new ByteConstant((byte)Value, ValueState);
            }
            else if (to == StdType.Short) result = this;
            else if (to == StdType.Int)
            {
                result = (Signed) ? new IntConstant((int)Value, ValueState)
                                          : new IntConstant((uint)Value, ValueState);
            }
            else if (to == StdType.Double)
            {
                result = new DoubleConstant((double)Value, ValueState);
            }

            return result;
        }
    }
}
