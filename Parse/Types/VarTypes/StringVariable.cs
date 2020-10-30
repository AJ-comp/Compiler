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

        public override bool CanAssign(IValue operand)
        {
            if (!Operation.CanOperation(this, operand)) return false;

            return true;
        }

        public override IConstant Assign(IValue operand)
        {
            if (!CanAssign(operand)) throw new FormatException();

            if (operand is IString)
            {
                string targetValue = operand.Value as string;
                return new StringConstant(targetValue);
            }
            else throw new NotSupportedException();
        }

        public IConstant Add(IValue operand) => Operation.StringAdd(this, operand);
        public IConstant Equal(IValue operand) => Operation.StringEqual(this, operand);
        public IConstant NotEqual(IValue operand) => Operation.StringNotEqual(this, operand);
    }
}
