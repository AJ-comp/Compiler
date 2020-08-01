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

        public IntConstant(IntConstant t) : base((int)t.Value, t.ValueState, t.PointerLevel)
        {
        }
    }
}
