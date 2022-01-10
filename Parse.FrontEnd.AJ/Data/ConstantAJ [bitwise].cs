using Parse.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Data
{
    public partial class ConstantAJ
    {
        public static ConstantAJ operator &(ConstantAJ source, ConstantAJ target)
        {
            if (source.Type.DataType == AJDataType.Bool) return source.BoolBitAnd(target);
            if (source.Type.IsIntegerType()) return source.IntegerBitAnd(target);

            throw new Exception();
        }

        public static ConstantAJ operator |(ConstantAJ source, ConstantAJ target)
        {
            if (source.Type.DataType == AJDataType.Bool) return source.BoolBitOr(target);
            if (source.Type.IsIntegerType()) return source.IntegerBitOr(target);

            throw new Exception();
        }

        public static ConstantAJ operator ~(ConstantAJ source)
        {
            if (source.Type.IsIntegerType())
            {
                if (source.ValueState == State.Fixed) return new ConstantAJ(~(int)source.Value);
                else return CreateValueUnknown(AJDataType.Int);
            }

            throw new Exception();
        }

        public static ConstantAJ operator ^(ConstantAJ source, ConstantAJ target)
        {
            if (source.Type.DataType == AJDataType.Bool) return source.BoolBitXor(target);
            if (source.Type.IsIntegerType()) return source.IntegerBitXor(target);

            throw new Exception();
        }

        /*
        public static ConstantAJ operator <<(ConstantAJ source, ConstantAJ count)
        {
            if (source.Type.IsIntegerType())
            {
                if (count.Type.IsIntegerType())
                {
                    if (source.ValueState == State.Fixed) return new ConstantAJ((int)source.Value << (int)count.Value);
                    else return CreateValueUnknown(AJDataType.Int);
                }
            }

            throw new Exception();
        }
        */


        private ConstantAJ BoolBitAnd(ConstantAJ target)
        {
            var state = ValueStateAfterCalc(target.ValueState);

            // bool & bool => bool
            if (target.Type.DataType == AJDataType.Bool)
            {
                if (state == State.Fixed) return new ConstantAJ((bool)Value & (bool)target.Value);
                else return CreateValueUnknown(AJDataType.Bool);
            }

            throw new Exception();
        }


        private ConstantAJ IntegerBitAnd(ConstantAJ target)
        {
            var state = ValueStateAfterCalc(target.ValueState);

            // integer & interget => interger
            if (target.Type.IsIntegerType())
            {
                if (state == State.Fixed) return new ConstantAJ((int)Value & (int)target.Value);
                else return CreateValueUnknown(AJDataType.Int);
            }

            throw new Exception();
        }


        private ConstantAJ BoolBitOr(ConstantAJ target)
        {
            var state = ValueStateAfterCalc(target.ValueState);

            // bool & bool => bool
            if (target.Type.DataType == AJDataType.Bool)
            {
                if (state == State.Fixed) return new ConstantAJ((bool)Value | (bool)target.Value);
                else return CreateValueUnknown(AJDataType.Bool);
            }

            throw new Exception();
        }


        private ConstantAJ IntegerBitOr(ConstantAJ target)
        {
            var state = ValueStateAfterCalc(target.ValueState);

            // integer & interget => interger
            if (target.Type.IsIntegerType())
            {
                if (state == State.Fixed) return new ConstantAJ((int)Value | (int)target.Value);
                else return CreateValueUnknown(AJDataType.Int);
            }

            throw new Exception();
        }


        private ConstantAJ BoolBitXor(ConstantAJ target)
        {
            var state = ValueStateAfterCalc(target.ValueState);

            // bool & bool => bool
            if (target.Type.DataType == AJDataType.Bool)
            {
                if (state == State.Fixed) return new ConstantAJ((bool)Value ^ (bool)target.Value);
                else return CreateValueUnknown(AJDataType.Bool);
            }

            throw new Exception();
        }


        private ConstantAJ IntegerBitXor(ConstantAJ target)
        {
            var state = ValueStateAfterCalc(target.ValueState);

            // integer & interget => interger
            if (target.Type.IsIntegerType())
            {
                if (state == State.Fixed) return new ConstantAJ((int)Value ^ (int)target.Value);
                else return CreateValueUnknown(AJDataType.Int);
            }

            throw new Exception();
        }
    }
}
