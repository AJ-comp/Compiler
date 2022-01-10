using Parse.Types.Operations;

namespace Parse.Types.ConstantTypes
{
    public class DoubleConstant : Constant, IDouble, ICompareOperation
    {
        public DoubleConstant(double value) : this(value, State.Fixed)
        {
            Nan = false;
        }

        public DoubleConstant(double value, State valueState) : base(value, valueState)
        {
        }

        public int Size => 64;
        public bool Nan { get; }

        public override StdType TypeKind => StdType.Double;
        public override bool AlwaysTrue => (ValueState == State.Fixed && (double)Value != 0);
        public override bool AlwaysFalse => (ValueState == State.Fixed && (double)Value == 0);

        public IConstant Add(IConstant operand) => Operation.ArithmeticAdd(this, operand);
        public IConstant Div(IConstant operand) => Operation.ArithmeticDiv(this, operand);
        public IConstant Equal(IConstant operand) => Operation.ArithmeticEqual(this, operand);
        public IConstant Mod(IConstant operand) => Operation.ArithmeticMod(this, operand);
        public IConstant Mul(IConstant operand) => Operation.ArithmeticMul(this, operand);
        public IConstant NotEqual(IConstant operand) => Operation.ArithmeticNotEqual(this, operand);
        public IConstant Sub(IConstant operand) => Operation.ArithmeticSub(this, operand);

        public IConstant GreaterThan(IConstant operand)
        {
            throw new System.NotImplementedException();
        }

        public IConstant LessThan(IConstant operand)
        {
            throw new System.NotImplementedException();
        }

        public IConstant GreaterEqual(IConstant operand)
        {
            throw new System.NotImplementedException();
        }

        public IConstant LessEqual(IConstant operand)
        {
            throw new System.NotImplementedException();
        }

        public override Constant Casting(StdType to)
        {
            Constant result = null;

            if (to == StdType.Bit)
            {
                var data = (int)Value != 0;
                result = new BitConstant(data, ValueState);
            }
            else if (to == StdType.Char) result = new ByteConstant((byte)Value, ValueState);
            else if (to == StdType.Short) result = new ShortConstant((short)Value, ValueState);
            else if (to == StdType.Int) result = new IntConstant((int)Value, ValueState);
            else if (to == StdType.Double) result = this;

            return result;
        }
    }
}
