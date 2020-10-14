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

        public IntConstant(int value, State valueState) : base(value, valueState)
        {
        }

        public IntConstant(uint value, State valueState) : base(value, valueState)
        {
        }

        public IntConstant(IntConstant t) : base((int)t.Value, t.ValueState)
        {
        }

        public override Constant Casting(DType to)
        {
            Constant result = null;

            if (to == DType.Bit)
            {
                var data = ((int)Value == 0) ? false : true;
                result = new BitConstant(data, ValueState);
            }
            else if (to == DType.Byte)
            {
                result = (Signed) ? new ByteConstant((sbyte)Value, ValueState)
                                          : new ByteConstant((byte)Value, ValueState);
            }
            else if (to == DType.Short)
            {
                result = (Signed) ? new ShortConstant((short)Value, ValueState)
                                          : new ShortConstant((ushort)Value, ValueState);
            }
            else if (to == DType.Int) result = this;
            else if (to == DType.Double)
            {
                result = new DoubleConstant((double)Value, ValueState);
            }

            return result;
        }
    }
}
