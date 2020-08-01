using Parse.Types.ConstantTypes;
using Parse.Types.Operations;
using System;

namespace Parse.Types.VarTypes
{
    public class StringVariable : Variable, IString
    {
        public int Size => throw new NotImplementedException();

        public override DType TypeName => DType.Unknown;


        public StringVariable(StringConstant value) : base(value)
        {
        }

        public override IConstant Assign(IValue operand)
        {
            if (!Operation.CanOperation(this, operand)) throw new FormatException();

            if (operand is IString)
            {
                string targetValue = operand.Value as string;
                return new StringConstant(PointerLevel, targetValue);
            }
            else throw new NotSupportedException();
        }

        public IConstant Add(IValue operand) => Operation.StringAdd(this, operand);
        public IConstant Equal(IValue operand) => Operation.StringEqual(this, operand);
        public IConstant NotEqual(IValue operand) => Operation.StringNotEqual(this, operand);
    }
}
