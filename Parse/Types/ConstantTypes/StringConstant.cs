using Parse.Types.Operations;

namespace Parse.Types.ConstantTypes
{
    public class StringConstant : Constant, IString
    {
        public StringConstant(string value) : base(value, State.Fixed, 0)
        {
        }

        public StringConstant(uint pointerLevel, string value) : base(value, State.Fixed, pointerLevel)
        {
        }

        public StringConstant(string value, State valueState, uint pointerLevel) : base(value, valueState, pointerLevel)
        {
        }

        public int Size => throw new System.NotImplementedException();

        public override DType TypeName => DType.Unknown;

        public IConstant Add(IValue operand) => Operation.StringAdd(this, operand);
        public IConstant Equal(IValue operand) => Operation.StringEqual(this, operand);
        public IConstant NotEqual(IValue operand) => Operation.StringNotEqual(this, operand);
    }
}
