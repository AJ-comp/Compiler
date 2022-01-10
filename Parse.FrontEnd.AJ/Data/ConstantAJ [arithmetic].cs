using Parse.Types;
using System;

namespace Parse.FrontEnd.AJ.Data
{
    public partial class ConstantAJ
    {
        public static ConstantAJ operator +(ConstantAJ source, ConstantAJ target)
        {
            if (source.Type.IsIntegerType()) return source.IntegerAdd(target);
            if (source.Type.DataType == AJDataType.Double) return source.FloatingAdd(target);
            if (source.Type.DataType == AJDataType.String) return source.StringAdd(target);

            throw new Exception();
        }

        public static ConstantAJ operator -(ConstantAJ source, ConstantAJ target)
        {
            if (source.Type.IsIntegerType()) return source.IntegerSub(target);
            if (source.Type.DataType == AJDataType.Double) return source.FloatingSub(target);

            throw new Exception();
        }

        public static ConstantAJ operator *(ConstantAJ source, ConstantAJ target)
        {
            if (source.Type.IsIntegerType()) return source.IntegerMul(target);
            if (source.Type.DataType == AJDataType.Double) return source.FloatingMul(target);

            throw new Exception();
        }

        public static ConstantAJ operator /(ConstantAJ source, ConstantAJ target)
        {
            if (source.Type.IsIntegerType()) return source.IntegerDiv(target);
            if (source.Type.DataType == AJDataType.Double) return source.FloatingDiv(target);

            throw new Exception();
        }

        public static ConstantAJ operator %(ConstantAJ source, ConstantAJ target)
        {
            if (source.Type.IsIntegerType()) return source.IntegerMod(target);
            if (source.Type.DataType == AJDataType.Double) return source.FloatingMod(target);

            throw new Exception();
        }


        /*******************************************/
        /// <summary>
        /// This function means the expression Integer + constant
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        /*******************************************/
        private ConstantAJ IntegerAdd(ConstantAJ target)
        {
            var state = ValueStateAfterCalc(target.ValueState);

            if (target.Type.IsIntegerType())
            {
                if (state == State.Fixed) return new ConstantAJ((int)Value + (int)target.Value);
                else return CreateValueUnknown(AJDataType.Int);
            }

            if (target.Type.IsFloatingType())
            {
                if (state == State.Fixed) return new ConstantAJ((int)Value + (double)target.Value);
                else return CreateValueUnknown(AJDataType.Double);
            }

            if (target.Type.DataType == AJDataType.String)
            {
                if (state == State.Fixed) return new ConstantAJ((int)Value + (string)target.Value);
                else return CreateValueUnknown(AJDataType.String);
            }

            throw new Exception();
        }


        /*******************************************/
        /// <summary>
        /// This function means the expression double + constant
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        /*******************************************/
        private ConstantAJ FloatingAdd(ConstantAJ target)
        {
            var state = ValueStateAfterCalc(target.ValueState);

            if (target.Type.IsArithmeticType())
            {
                if (state == State.Fixed) return new ConstantAJ((double)Value + (double)target.Value);
                else return CreateValueUnknown(AJDataType.Double);
            }

            if (target.Type.DataType == AJDataType.String)
            {
                if (state == State.Fixed) return new ConstantAJ((double)Value + (string)target.Value);
                else return CreateValueUnknown(AJDataType.String);
            }

            throw new Exception();
        }


        /*******************************************/
        /// <summary>
        /// This function means the expression string + constant
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        /*******************************************/
        private ConstantAJ StringAdd(ConstantAJ target)
        {
            var state = ValueStateAfterCalc(target.ValueState);

            if (target.Type.IsArithmeticType())
            {
                if (state == State.Fixed) return new ConstantAJ((string)Value + (double)target.Value);
                else return CreateValueUnknown(AJDataType.String);
            }

            if (target.Type.DataType == AJDataType.String)
            {
                if (state == State.Fixed) return new ConstantAJ((string)Value + (string)target.Value);
                else return CreateValueUnknown(AJDataType.String);
            }

            throw new Exception();
        }


        /*******************************************/
        /// <summary>
        /// This function means the expression integer - constant
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        /*******************************************/
        private ConstantAJ IntegerSub(ConstantAJ target)
        {
            var state = ValueStateAfterCalc(target.ValueState);

            if (target.Type.IsIntegerType())
            {
                if (state == State.Fixed) return new ConstantAJ((int)Value - (int)target.Value);
                else return CreateValueUnknown(AJDataType.Int);
            }

            if (target.Type.IsFloatingType())
            {
                if (state == State.Fixed) return new ConstantAJ((int)Value - (double)target.Value);
                else return CreateValueUnknown(AJDataType.Double);
            }

            throw new Exception();
        }


        /*******************************************/
        /// <summary>
        /// This function means the expression double - constant
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        /*******************************************/
        private ConstantAJ FloatingSub(ConstantAJ target)
        {
            var state = ValueStateAfterCalc(target.ValueState);

            if (target.Type.IsArithmeticType())
            {
                if (state == State.Fixed) return new ConstantAJ((double)Value - (double)target.Value);
                else return CreateValueUnknown(AJDataType.Double);
            }

            throw new Exception();
        }


        /*******************************************/
        /// <summary>
        /// This function means the expression integer * constant
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        /*******************************************/
        private ConstantAJ IntegerMul(ConstantAJ target)
        {
            var state = ValueStateAfterCalc(target.ValueState);

            if (target.Type.IsIntegerType())
            {
                if (state == State.Fixed) return new ConstantAJ((int)Value * (int)target.Value);
                else return CreateValueUnknown(AJDataType.Int);
            }

            if (target.Type.IsFloatingType())
            {
                if (state == State.Fixed) return new ConstantAJ((int)Value * (double)target.Value);
                else return CreateValueUnknown(AJDataType.Double);
            }

            throw new Exception();
        }


        /*******************************************/
        /// <summary>
        /// This function means the expression double * constant
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        /*******************************************/
        private ConstantAJ FloatingMul(ConstantAJ target)
        {
            var state = ValueStateAfterCalc(target.ValueState);

            if (target.Type.IsArithmeticType())
            {
                if (state == State.Fixed) return new ConstantAJ((double)Value * (double)target.Value);
                else return CreateValueUnknown(AJDataType.Double);
            }

            throw new Exception();
        }


        /*******************************************/
        /// <summary>
        /// This function means the expression integer / constant
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        /*******************************************/
        private ConstantAJ IntegerDiv(ConstantAJ target)
        {
            var state = ValueStateAfterCalc(target.ValueState);

            if (target.Type.IsIntegerType())
            {
                if (state == State.Fixed) return new ConstantAJ((int)Value / (int)target.Value);
                else return CreateValueUnknown(AJDataType.Int);
            }

            if (target.Type.IsFloatingType())
            {
                if (state == State.Fixed) return new ConstantAJ((int)Value / (double)target.Value);
                else return CreateValueUnknown(AJDataType.Double);
            }

            throw new Exception();
        }


        /*******************************************/
        /// <summary>
        /// This function means the expression double / constant
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        /*******************************************/
        private ConstantAJ FloatingDiv(ConstantAJ target)
        {
            var state = ValueStateAfterCalc(target.ValueState);

            if (target.Type.IsArithmeticType())
            {
                if (state == State.Fixed) return new ConstantAJ((double)Value / (double)target.Value);
                else return CreateValueUnknown(AJDataType.Double);
            }

            throw new Exception();
        }


        /*******************************************/
        /// <summary>
        /// This function means the expression integer % constant
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        /*******************************************/
        private ConstantAJ IntegerMod(ConstantAJ target)
        {
            var state = ValueStateAfterCalc(target.ValueState);

            if (target.Type.IsIntegerType())
            {
                if (state == State.Fixed) return new ConstantAJ((int)Value % (int)target.Value);
                else return CreateValueUnknown(AJDataType.Int);
            }

            if (target.Type.IsFloatingType())
            {
                if (state == State.Fixed) return new ConstantAJ((int)Value % (double)target.Value);
                else return CreateValueUnknown(AJDataType.Double);
            }

            throw new Exception();
        }


        /*******************************************/
        /// <summary>
        /// This function means the expression double % constant
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        /*******************************************/
        private ConstantAJ FloatingMod(ConstantAJ target)
        {
            var state = ValueStateAfterCalc(target.ValueState);

            if (target.Type.IsArithmeticType())
            {
                if (state == State.Fixed) return new ConstantAJ((double)Value % (double)target.Value);
                else return CreateValueUnknown(AJDataType.Double);
            }

            throw new Exception();
        }
    }
}
