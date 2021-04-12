using Parse.Types.ConstantTypes;
using Parse.Types.Operations;
using System;

namespace Parse.Types.VarTypes
{
    public class StringVariable : Variable, IString
    {
        public int Size => throw new NotImplementedException();

        public override StdType TypeKind => StdType.Unknown;


        public StringVariable(StringConstant value) : base(value)
        {
        }

        public override bool CanAssign(IConstant operand)
        {
            if (!Operation.CanOperation(ValueConstant, operand)) return false;

            return true;
        }

        public override IConstant Assign(IConstant operand)
        {
            if (!CanAssign(operand)) throw new FormatException();

            if (operand is IString)
            {
                string targetValue = operand.Value as string;
                return new StringConstant(targetValue);
            }
            else throw new NotSupportedException();
        }

        public IConstant Add(IConstant operand) => Operation.StringAdd(ValueConstant, operand);
        public IConstant Equal(IConstant operand) => Operation.StringEqual(ValueConstant, operand);
        public IConstant NotEqual(IConstant operand) => Operation.StringNotEqual(ValueConstant, operand);
    }
}
