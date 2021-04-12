using Parse.Types.Operations;
using System;

namespace Parse.Types.ConstantTypes
{
    public abstract class Constant : IConstant
    {
        public object Value { get; set; }
        public State ValueState { get; protected set; } = State.NotInit;
        public abstract StdType TypeKind { get; }
        public abstract bool AlwaysTrue { get; }
        public abstract bool AlwaysFalse { get; }


        protected Constant(object value, State valueState)
        {
            Value = value;
            ValueState = valueState;
        }

        public abstract Constant Casting(StdType to);

        public static IConstant Add(IConstant constant, IConstant operand)
        {
            if (constant is IArithmeticOperation) return (constant as IArithmeticOperation).Add(operand);
            // in add case, it also has to support string type.
            if (constant is IString) return (constant as IString).Add(operand);

            return new UnknownConstant();
        }

        public static IConstant Sub(IConstant constant, IConstant operand)
        {
            if (constant is IArithmeticOperation) return (constant as IArithmeticOperation).Sub(operand);

            return new UnknownConstant();
        }

        public static IConstant Mul(IConstant constant, IConstant operand)
        {
            if (constant is IArithmeticOperation) return (constant as IArithmeticOperation).Mul(operand);

            return new UnknownConstant();
        }

        public static IConstant Div(IConstant constant, IConstant operand)
        {
            if (constant is IArithmeticOperation) return (constant as IArithmeticOperation).Div(operand);

            return new UnknownConstant();
        }

        public static IConstant Mod(IConstant constant, IConstant operand)
        {
            if (constant is IArithmeticOperation) return (constant as IArithmeticOperation).Mod(operand);

            return new UnknownConstant();
        }




        public static IConstant And(IConstant constant, IConstant operand)
        {
            if (constant is ILogicalOperation) return (constant as ILogicalOperation).And(operand);

            return new UnknownConstant();
        }

        public static IConstant Or(IConstant constant, IConstant operand)
        {
            if (constant is ILogicalOperation) return (constant as ILogicalOperation).Or(operand);

            return new UnknownConstant();
        }

        public static IConstant Not(IConstant constant)
        {
            if (constant is ILogicalOperation) return (constant as ILogicalOperation).Not();

            return new UnknownConstant();
        }



        public static IConstant BitAnd(IConstant constant, IConstant operand)
        {
            if (constant is IBitwiseOperation) return (constant as IBitwiseOperation).BitAnd(operand);

            return new UnknownConstant();
        }

        public static IConstant BitOr(IConstant constant, IConstant operand)
        {
            if (constant is IBitwiseOperation) return (constant as IBitwiseOperation).BitOr(operand);

            return new UnknownConstant();
        }

        public static IConstant BitXor(IConstant constant, IConstant operand)
        {
            if (constant is IBitwiseOperation) return (constant as IBitwiseOperation).BitXor(operand);

            return new UnknownConstant();
        }

        public static IConstant BitNot(IConstant constant)
        {
            if (constant is IBitwiseOperation) return (constant as IBitwiseOperation).BitNot();

            return new UnknownConstant();
        }

        public static IConstant LeftShift(IConstant constant, int count)
        {
            if (constant is IBitwiseOperation) return (constant as IBitwiseOperation).LeftShift(count);

            return new UnknownConstant();
        }

        public static IConstant RightShift(IConstant constant, int count)
        {
            if (constant is IBitwiseOperation) return (constant as IBitwiseOperation).RightShift(count);

            return new UnknownConstant();
        }




        public static IConstant Equal(IConstant constant, IConstant operand)
        {
            if (constant is IEqualOperation) return (constant as IEqualOperation).Equal(operand);

            return new UnknownConstant();
        }

        public static IConstant NotEqual(IConstant constant, IConstant operand)
        {
            if (constant is IEqualOperation) return (constant as IEqualOperation).NotEqual(operand);

            return new UnknownConstant();
        }



        public static IConstant GreaterThan(IConstant constant, IConstant operand)
        {
            if (constant is ISizeCompareOperation) return (constant as ISizeCompareOperation).GreaterThan(operand);

            return new UnknownConstant();
        }

        public static IConstant LessThan(IConstant constant, IConstant operand)
        {
            if (constant is ISizeCompareOperation) return (constant as ISizeCompareOperation).LessThan(operand);

            return new UnknownConstant();
        }

        public static IConstant GreaterEqual(IConstant constant, IConstant operand)
        {
            if (constant is ISizeCompareOperation) return (constant as ISizeCompareOperation).GreaterEqual(operand);

            return new UnknownConstant();
        }

        public static IConstant LessEqual(IConstant constant, IConstant operand)
        {
            if (constant is ISizeCompareOperation) return (constant as ISizeCompareOperation).LessEqual(operand);

            return new UnknownConstant();
        }
    }
}
