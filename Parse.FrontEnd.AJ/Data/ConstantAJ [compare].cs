using Parse.Types;
using System;

namespace Parse.FrontEnd.AJ.Data
{
    public partial class ConstantAJ
    {
        public static ConstantAJ operator >=(ConstantAJ source, ConstantAJ target)
        {
            if (source.Type.IsArithmeticType()) return source.Compare(target, IRCompareOperation.GE);

            throw new Exception();
        }

        public static ConstantAJ operator >(ConstantAJ source, ConstantAJ target)
        {
            if (source.Type.IsArithmeticType()) return source.Compare(target, IRCompareOperation.GT);

            throw new Exception();
        }

        public static ConstantAJ operator <=(ConstantAJ source, ConstantAJ target)
        {
            if (source.Type.IsArithmeticType()) return source.Compare(target, IRCompareOperation.LE);

            throw new Exception();
        }

        public static ConstantAJ operator <(ConstantAJ source, ConstantAJ target)
        {
            if (source.Type.IsArithmeticType()) return source.Compare(target, IRCompareOperation.LT);

            throw new Exception();
        }


        public ConstantAJ EQ(ConstantAJ target)
        {
            if (Type.IsArithmeticType()) return ArithmeticEqual(target);
            if (Type.DataType == AJDataType.Bool) return BoolEqual(target);
            if (Type.DataType == AJDataType.String) return StringEqual(target);

            throw new Exception();
        }


        public ConstantAJ NotEQ(ConstantAJ target)
        {
            var result = EQ(target);
            if (result.ValueState == State.Fixed)
                result.Value = !(bool)result.Value;

            return result;
        }


        private ConstantAJ Compare(ConstantAJ target, IRCompareOperation compare)
        {
            var state = ValueStateAfterCalc(target.ValueState);

            if (target.Type.IsArithmeticType())
            {
                if (state == State.Fixed)
                {
                    if (compare == IRCompareOperation.GE) return new ConstantAJ((double)Value >= (double)target.Value);
                    if (compare == IRCompareOperation.GT) return new ConstantAJ((double)Value > (double)target.Value);
                    if (compare == IRCompareOperation.LE) return new ConstantAJ((double)Value <= (double)target.Value);
                    if (compare == IRCompareOperation.LT) return new ConstantAJ((int)Value < (double)target.Value);
                }
                else return CreateValueUnknown(AJDataType.Bool);
            }

            throw new Exception();
        }


        private ConstantAJ ArithmeticEqual(ConstantAJ target)
        {
            var state = ValueStateAfterCalc(target.ValueState);

            if (target.Type.IsArithmeticType())
            {
                if (state == State.Fixed) return new ConstantAJ((double)Value == (double)target.Value);
                else return CreateValueUnknown(AJDataType.Bool);
            }

            throw new Exception();
        }


        private ConstantAJ BoolEqual(ConstantAJ target)
        {
            var state = ValueStateAfterCalc(target.ValueState);

            if (target.Type.DataType == AJDataType.Bool)
            {
                if (state == State.Fixed) return new ConstantAJ((bool)Value == (bool)target.Value);
                else return CreateValueUnknown(AJDataType.Bool);
            }

            throw new Exception();
        }


        private ConstantAJ StringEqual(ConstantAJ target)
        {
            var state = ValueStateAfterCalc(target.ValueState);

            if (target.Type.DataType == AJDataType.String)
            {
                if (state == State.Fixed) return new ConstantAJ((string)Value == (string)target.Value);
                else return CreateValueUnknown(AJDataType.Bool);
            }

            throw new Exception();
        }
    }
}
