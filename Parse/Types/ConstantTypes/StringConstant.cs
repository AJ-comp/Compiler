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

        public override StdType TypeKind => StdType.Unknown;
        public override bool AlwaysTrue => (ValueState == State.Fixed && Value.ToString().Length > 0);
        public override bool AlwaysFalse => false;

        public IConstant Add(IConstant operand) => Operation.StringAdd(this, operand);
        public IConstant Equal(IConstant operand) => Operation.StringEqual(this, operand);
        public IConstant NotEqual(IConstant operand) => Operation.StringNotEqual(this, operand);

        public override Constant Casting(StdType to)
        {
            return null;
        }
    }
}
