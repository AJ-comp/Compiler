using Parse.Types.Operations;

namespace Parse.Types.ConstantTypes
{
    public class BitConstant : Constant, IBit
    {
        public override StdType TypeKind => StdType.Bit;
        public override bool AlwaysTrue => (ValueState == State.Fixed && (bool)Value == true);
        public override bool AlwaysFalse => (ValueState == State.Fixed && (bool)Value == false);

        public BitConstant(bool value) : this(value, State.Fixed)
        {
        }

        public BitConstant(bool value, State valueState) : base(value, valueState)
        {
        }

        public BitConstant(BitConstant t) : base(t.Value, t.ValueState)
        {
        }

        public  IConstant And(IConstant operand) => Operation.BitTypeAnd(this, operand);
        public  IConstant BitAnd(IConstant operand) => Operation.BitTypeBitAnd(this, operand);
        public  IConstant BitNot() => Operation.BitTypeBitNot(this);
        public  IConstant BitOr(IConstant operand) => Operation.BitTypeBitOr(this, operand);
        public  IConstant BitXor(IConstant operand) => Operation.BitTypeBitXor(this, operand);
        public  IConstant Equal(IConstant operand) => Operation.BitTypeEqual(this, operand);
        public  IConstant LeftShift(int count) => Operation.BitTypeLeftShift(this, count);
        public  IConstant Not() => Operation.BitTypeNot(this);
        public  IConstant NotEqual(IConstant operand) => Operation.BitTypeNotEqual(this, operand);
        public  IConstant Or(IConstant operand) => Operation.BitTypeOr(this, operand);
        public  IConstant RightShift(int count) => Operation.BitTypeRightShift(this, count);

        public override Constant Casting(StdType to)
        {
            Constant result = null;

            if (to == StdType.Bit) result = this;
            else if (to == StdType.Byte) result = new ByteConstant((byte)Value, ValueState);
            else if (to == StdType.Int) result = new IntConstant((int)Value, ValueState);
            else if (to == StdType.Double) result = new DoubleConstant((double)Value, ValueState);

            return result;
        }
    }
}
