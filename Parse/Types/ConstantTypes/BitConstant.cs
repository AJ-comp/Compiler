using Parse.Types.Operations;

namespace Parse.Types.ConstantTypes
{
    public class BitConstant : Constant, IBit
    {
        public override DType TypeName => DType.Bit;

        public BitConstant(bool value) : this(value, State.Fixed, 0)
        {
        }

        public BitConstant(uint pointerLevel, bool value) : this(value, State.Fixed, pointerLevel)
        {
        }

        public BitConstant(bool value, State valueState, uint pointerLevel) : base(value, valueState, pointerLevel)
        {
        }

        public BitConstant(BitConstant t) : base(t.Value, t.ValueState, t.PointerLevel)
        {
        }

        public IConstant And(IValue operand) => Operation.BitTypeAnd(this, operand);
        public IConstant BitAnd(IValue operand) => Operation.BitTypeBitAnd(this, operand);
        public IConstant BitNot() => Operation.BitTypeBitNot(this);
        public IConstant BitOr(IValue operand) => Operation.BitTypeBitOr(this, operand);
        public IConstant BitXor(IValue operand) => Operation.BitTypeBitXor(this, operand);
        public IConstant Equal(IValue operand) => Operation.BitTypeEqual(this, operand);
        public IConstant LeftShift(int count) => Operation.BitTypeLeftShift(this, count);
        public IConstant Not() => Operation.BitTypeNot(this);
        public IConstant NotEqual(IValue operand) => Operation.BitTypeNotEqual(this, operand);
        public IConstant Or(IValue operand) => Operation.BitTypeOr(this, operand);
        public IConstant RightShift(int count) => Operation.BitTypeRightShift(this, count);

        public override Constant Casting(DType to)
        {
            Constant result = null;

            if (to == DType.Bit) result = this;
            else if (to == DType.Byte) result = new ByteConstant((byte)Value, ValueState, PointerLevel);
            else if (to == DType.Int) result = new IntConstant((int)Value, ValueState, PointerLevel);
            else if (to == DType.Double) result = new DoubleConstant((double)Value, ValueState, PointerLevel);

            return result;
        }
    }
}
