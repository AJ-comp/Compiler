using Parse.Types.Operations;

namespace Parse.Types.ConstantTypes
{
    public class StringConstant : Constant, IString
    {
        public StringConstant(string value) : base(value, State.Fixed)
        {
        }

        public StringConstant(string value, State valueState) : base(value, valueState)
        {
        }

        public int Size => throw new System.NotImplementedException();

        public override DType TypeName => DType.Unknown;
        public override bool AlwaysTrue => (ValueState == State.Fixed && Value.ToString().Length > 0);
        public override bool AlwaysFalse => false;

        public IConstant Add(IValue operand) => Operation.StringAdd(this, operand);
        public IConstant Equal(IValue operand) => Operation.StringEqual(this, operand);
        public IConstant NotEqual(IValue operand) => Operation.StringNotEqual(this, operand);

        public override Constant Casting(DType to)
        {
            return null;
        }
    }
}
