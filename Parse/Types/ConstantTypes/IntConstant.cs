namespace Parse.Types.ConstantTypes
{
    public class IntConstant : IntegerConstant, IInt
    {
        public int RealValue => (int)Value;
        public override int Size => 32;
        public override StdType TypeKind => StdType.Int;

        public IntConstant(int value) : base(value)
        {
        }

        public IntConstant(uint value) : base(value)
        {
        }

        public IntConstant(int value, State valueState) : base(value, valueState)
        {
        }

        public IntConstant(uint value, State valueState) : base(value, valueState)
        {
        }

        public IntConstant(IntConstant t) : base((int)t.Value, t.ValueState)
        {
        }

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
            else if (to == StdType.Short)
            {
                result = (Signed) ? new ShortConstant((short)Value, ValueState)
                                          : new ShortConstant((ushort)Value, ValueState);
            }
            else if (to == StdType.Int) result = this;
            else if (to == StdType.Double)
            {
                result = new DoubleConstant((double)Value, ValueState);
            }

            return result;
        }
    }
}
