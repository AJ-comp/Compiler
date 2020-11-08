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

        public IConstant Equal(IValue operand) => Operation.ArithmeticEqual(this, operand);
        public IConstant NotEqual(IValue operand) => Operation.ArithmeticNotEqual(this, operand);
        public IConstant Add(IValue operand) => Operation.ArithmeticAdd(this, operand);
        public IConstant Sub(IValue operand) => Operation.ArithmeticSub(this, operand);
        public IConstant Mul(IValue operand) => Operation.ArithmeticMul(this, operand);
        public IConstant Div(IValue operand) => Operation.ArithmeticDiv(this, operand);
        public IConstant Mod(IValue operand) => Operation.ArithmeticMod(this, operand);
        public IConstant BitAnd(IValue operand) => Operation.IntegerKindBitAnd(this, operand);
        public IConstant BitOr(IValue operand) => Operation.IntegerKindBitOr(this, operand);
        public IConstant BitNot() => Operation.IntegerKindBitNot(this);
        public IConstant BitXor(IValue operand) => Operation.IntegerKindBitXor(this, operand);
        public IConstant LeftShift(int count) => Operation.IntegerKindLeftShift(this, count);
        public IConstant RightShift(int count) => Operation.IntegerKindRightShift(this, count);
    }
}
