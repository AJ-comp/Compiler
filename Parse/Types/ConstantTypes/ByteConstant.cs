namespace Parse.Types.ConstantTypes
{
    public class ByteConstant : IntegerConstant, IByte
    {
        public ByteConstant(sbyte value) : base(value)
        {
        }

        public ByteConstant(byte value) : base(value)
        {
        }

        public ByteConstant(byte value, State valueState) : base(value, valueState)
        {
        }

        public ByteConstant(sbyte value, State valueState) : base(value, valueState)
        {
        }

        public ByteConstant(ByteConstant t) : base((byte)t.Value, t.ValueState)
        {
        }

        public override int Size => 8;
        public override StdType TypeKind => StdType.Char;

        public override Constant Casting(StdType to)
        {
            Constant result = null;

            if (to == StdType.Bit) result = this;
            else if (to == StdType.Char) result = new ByteConstant((byte)Value, ValueState);
            else if (to == StdType.Int) result = new IntConstant((int)Value, ValueState);
            else if (to == StdType.Double) result = new DoubleConstant((double)Value, ValueState);

            return result;
        }
    }
}
