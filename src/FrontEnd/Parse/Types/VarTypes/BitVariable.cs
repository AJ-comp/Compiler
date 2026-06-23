using Parse.Types.ConstantTypes;
using Parse.Types.Operations;
using System;

namespace Parse.Types.VarTypes
{
    public class BitVariable : Variable, IBit
    {
        public override StdType TypeKind => StdType.Bit;

        public BitVariable(BitConstant constant) : base(constant)
        {
        }

        public override bool CanAssign(IConstant operand)
        {
            if (!Operation.CanOperation(ValueConstant, operand)) return false;
            if (!(operand is IBit)) return false;

            return true;
        }

        public override IConstant Assign(IConstant operand)
        {
            if(!CanAssign(operand)) throw new NotSupportedException();

            if (operand is IVariable)
                ValueConstant = (operand as IVariable).ValueConstant;
            else
                ValueConstant = (operand as IConstant);

            return new BitConstant(ValueConstant as BitConstant);
        }

        public IConstant And(IConstant operand) => Operation.BitTypeAnd(ValueConstant, operand);
        public IConstant BitAnd(IConstant operand) => Operation.BitTypeBitAnd(ValueConstant, operand);
        public IConstant BitNot() => Operation.BitTypeBitNot(ValueConstant);
        public IConstant BitOr(IConstant operand) => Operation.BitTypeBitOr(ValueConstant, operand);
        public IConstant BitXor(IConstant operand) => Operation.BitTypeBitXor(ValueConstant, operand);
        public IConstant Equal(IConstant operand) => Operation.BitTypeEqual(ValueConstant, operand);
        public IConstant LeftShift(int count) => Operation.BitTypeLeftShift(ValueConstant, count);
        public IConstant Not() => Operation.BitTypeNot(ValueConstant);
        public IConstant NotEqual(IConstant operand) => Operation.BitTypeNotEqual(ValueConstant, operand);
        public IConstant Or(IConstant operand) => Operation.BitTypeOr(ValueConstant, operand);
        public IConstant RightShift(int count) => Operation.BitTypeRightShift(ValueConstant, count);
    }
}
