using Parse.Types.ConstantTypes;
using Parse.Types.Operations;
using System;

namespace Parse.Types.VarTypes
{
    public class BitVariable : Variable, IBit
    {
        public override DType TypeName => DType.Bit;

        public BitVariable(BitConstant constant) : base(constant)
        {
        }

        public override bool CanAssign(IValue operand)
        {
            if (!Operation.CanOperation(this, operand)) return false;
            if (!(operand is IBit)) return false;

            return true;
        }

        public override IConstant Assign(IValue operand)
        {
            if(!CanAssign(operand)) throw new NotSupportedException();

            if (operand is IVariable)
                ValueConstant = (operand as IVariable).ValueConstant;
            else
                ValueConstant = (operand as IConstant);

            return new BitConstant(ValueConstant as BitConstant);
        }

        public IConstant And(IValue operand) => Operation.BitTypeAnd(this, operand);
        public IConstant BitAnd(IValue operand) => Operation.BitTypeBitAnd(this, operand);
        public IConstant BitNot() => Operation.BitTypeBitNot(this);
        public IConstant BitOr(IValue operand) => Operation.BitTypeBitOr(this, operand);
        public IConstant BitXor(IValue operand) => Operation.BitTypeBitXor(this, operand);
        public IConstant Equal(IValue operand) => Operation.BitTypeEqual(this, operand);
        public IConstant LeftShift(int count) => Operation.BitTypeLeftShift(this, count);
        public IConstant Not() => Operation.BitTypeNot(this);
        public IConstant NotEqual(IValue operand) => Operation.BitTypeNotEqual(this, operand);
        public IConstant Or(IValue operand) => Operation.BitTypeOr(this, operand);
        public IConstant RightShift(int count) => Operation.BitTypeRightShift(this, count);
    }
}
