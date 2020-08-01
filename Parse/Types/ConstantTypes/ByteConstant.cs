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

        public ByteConstant(uint pointerLevel, byte value) : base(pointerLevel, value)
        {
        }

        public ByteConstant(byte value, State valueState, uint pointerLevel) : base(value, valueState, pointerLevel)
        {
        }

        public ByteConstant(ByteConstant t) : base((byte)t.Value, t.ValueState, t.PointerLevel)
        {
        }

        public override int Size => 8;
        public override DType TypeName => DType.Byte;
    }
}
