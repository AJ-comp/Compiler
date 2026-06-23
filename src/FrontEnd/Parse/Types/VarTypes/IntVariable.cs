using Parse.Types.ConstantTypes;
using Parse.Types.Operations;
using System;

namespace Parse.Types.VarTypes
{
    public class IntVariable : Variable, IInt
    {
        public int RealValue => (int)Value;
        public int Size => 32;
        public bool Signed { get; }

        public override StdType TypeKind => StdType.Int;

        public IntVariable(IntConstant value) : base(value)
        {
            Signed = value.Signed;
        }

        public override bool CanAssign(IConstant operand)
        {
            if (!Operation.CanOperation(ValueConstant, operand)) return false;
            if (!(operand is IIntegerKind)) return false;

            return true;
        }

        public override IConstant Assign(IConstant operand)
        {
            if(!CanAssign(operand)) throw new NotSupportedException();

            if (operand is IVariable)
            {
                var valueConstant = (operand as IVariable).ValueConstant;

                // operand may be not int type so it has to make int type explicity.
                ValueConstant = new IntConstant((int)ValueConstant.Value, 
                                                                    ValueConstant.ValueState);
            }
            else
            {
                var valueConstant = (operand as IConstant);

                // operand may be not int type so it has to make int type explicity.
                ValueConstant = new IntConstant((int)ValueConstant.Value,
                                                                    ValueConstant.ValueState);
            }

            return ValueConstant;
        }

        public IConstant Equal(IConstant operand) => Operation.ArithmeticEqual(ValueConstant, operand);
        public IConstant NotEqual(IConstant operand) => Operation.ArithmeticNotEqual(ValueConstant, operand);
        public IConstant Add(IConstant operand) => Operation.ArithmeticAdd(ValueConstant, operand);
        public IConstant Sub(IConstant operand) => Operation.ArithmeticSub(ValueConstant, operand);
        public IConstant Mul(IConstant operand) => Operation.ArithmeticMul(ValueConstant, operand);
        public IConstant Div(IConstant operand) => Operation.ArithmeticDiv(ValueConstant, operand);
        public IConstant Mod(IConstant operand) => Operation.ArithmeticMod(ValueConstant, operand);
        public IConstant BitAnd(IConstant operand) => Operation.IntegerKindBitAnd(ValueConstant, operand);
        public IConstant BitOr(IConstant operand) => Operation.IntegerKindBitOr(ValueConstant, operand);
        public IConstant BitNot() => Operation.IntegerKindBitNot(ValueConstant);
        public IConstant BitXor(IConstant operand) => Operation.IntegerKindBitXor(ValueConstant, operand);
        public IConstant LeftShift(int count) => Operation.IntegerKindLeftShift(ValueConstant, count);
        public IConstant RightShift(int count) => Operation.IntegerKindRightShift(ValueConstant, count);
    }
}
