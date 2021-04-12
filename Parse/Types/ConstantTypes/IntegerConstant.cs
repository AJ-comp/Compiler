using Parse.Types.Operations;

namespace Parse.Types.ConstantTypes
{
    public abstract class IntegerConstant : Constant, IIntegerKind
    {
        public bool Signed { get; protected set; }
        public abstract int Size { get; }

        public override bool AlwaysTrue => (ValueState == State.Fixed && (int)Value != 0);
        public override bool AlwaysFalse => (ValueState == State.Fixed && (int)Value == 0);

        protected IntegerConstant(int value) : base(value, State.Fixed)
        {
            Signed = true;
        }

        protected IntegerConstant(uint value) : base(value, State.Fixed)
        {
            Signed = false;
        }

        protected IntegerConstant(int value, State valueState) : base(value, valueState)
        {
            Signed = true;
        }

        protected IntegerConstant(uint value, State valueState) : base(value, valueState)
        {
            Signed = true;
        }

        public IConstant Equal(IConstant operand) => Operation.ArithmeticEqual(this, operand);
        public IConstant NotEqual(IConstant operand) => Operation.ArithmeticNotEqual(this, operand);
        public IConstant Add(IConstant operand) => Operation.ArithmeticAdd(this, operand);
        public IConstant Sub(IConstant operand) => Operation.ArithmeticSub(this, operand);
        public IConstant Mul(IConstant operand) => Operation.ArithmeticMul(this, operand);
        public IConstant Div(IConstant operand) => Operation.ArithmeticDiv(this, operand);
        public IConstant Mod(IConstant operand) => Operation.ArithmeticMod(this, operand);
        public IConstant BitAnd(IConstant operand) => Operation.IntegerKindBitAnd(this, operand);
        public IConstant BitOr(IConstant operand) => Operation.IntegerKindBitOr(this, operand);
        public IConstant BitNot() => Operation.IntegerKindBitNot(this);
        public IConstant BitXor(IConstant operand) => Operation.IntegerKindBitXor(this, operand);
        public IConstant LeftShift(int count) => Operation.IntegerKindLeftShift(this, count);
        public IConstant RightShift(int count) => Operation.IntegerKindRightShift(this, count);
    }
}
