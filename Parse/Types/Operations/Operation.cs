using Parse.Types.ConstantTypes;
using System;

namespace Parse.Types.Operations
{
    public class Operation
    {
        public static bool CanOperation(IConstant operand1, IConstant operand2)
        {
            return true;
        }


        #region The operation related to Bit type
        public static IConstant BitTypeAnd(IConstant operand1, IConstant operand2)
        {
            try
            {
                return new BitConstant((bool)operand1.Value && (bool)operand2.Value);
            }
            catch
            {
                return new UnknownConstant();
            }
        }

        public static IConstant BitTypeNot(IConstant operand) => new BitConstant(!(bool)operand.Value);

        public static IConstant BitTypeBitAnd(IConstant operand1, IConstant operand2)
        {
            try
            {
                return new BitConstant((bool)operand1.Value & (bool)operand2.Value);
            }
            catch
            {
                return new UnknownConstant();
            }
        }

        public static IConstant BitTypeBitNot(IConstant operand) => new UnknownConstant();

        public static IConstant BitTypeBitOr(IConstant operand1, IConstant operand2)
        {
            try
            {
                return new BitConstant((bool)operand1.Value | (bool)operand2.Value);
            }
            catch
            {
                return new UnknownConstant();
            }
        }

        public static IConstant BitTypeBitXor(IConstant operand1, IConstant operand2)
        {
            try
            {
                return new BitConstant((bool)operand1.Value ^ (bool)operand2.Value);
            }
            catch
            {
                return new UnknownConstant();
            }
        }

        public static IConstant BitTypeEqual(IConstant operand1, IConstant operand2)
        {
            try
            {
                return new BitConstant(operand1.Value == operand2.Value &&
                                  operand1.IsInitialized == operand2.IsInitialized);
            }
            catch
            {
                return new UnknownConstant();
            }
        }

        public static IConstant BitTypeNotEqual(IConstant operand1, IConstant operand2) => new UnknownConstant();

        public static IConstant BitTypeOr(IConstant operand1, IConstant operand2)
        {
            try
            {
                return new BitConstant((bool)operand1.Value || (bool)operand2.Value);
            }
            catch
            {
                return new UnknownConstant();
            }
        }

        public static IConstant BitTypeLeftShift(IConstant operand, int count) => new UnknownConstant();
        public static IConstant BitTypeRightShift(IConstant operand, int count) => new UnknownConstant();
        #endregion



        #region The operation realted to Arithmetic type (Integer + double)
        public static IConstant ArithmeticEqual(IConstant operand1, IConstant operand2)
        {
            try
            {
                bool condition = (operand2 is IArithmetic);

                if (condition) return new BitConstant((int)operand1.Value == (int)operand2.Value);
                if (operand2 is IDouble) return new BitConstant((double)operand1.Value == (double)operand2.Value);

                return new UnknownConstant();
            }
            catch
            {
                return new UnknownConstant();
            }
        }

        public static IConstant ArithmeticNotEqual(IConstant operand1, IConstant operand2)
        {
            try
            {
                bool condition = (operand2 is IArithmetic);

                if (condition) return new BitConstant((int)operand1.Value != (int)operand2.Value);
                if (operand2 is IDouble) return new BitConstant((double)operand1.Value != (double)operand2.Value);

                return new UnknownConstant();
            }
            catch
            {
                return new UnknownConstant();
            }
        }


        public static IConstant ArithmeticAdd(IConstant operand1, IConstant operand2)
        {
            try
            {
                // case string
                if (operand2 is IString)
                {
                    var strValue = operand1.Value.ToString() + operand2.Value.ToString();

                    return new StringConstant(strValue);
                }

                double value = Convert.ToDouble(operand1.Value) + Convert.ToDouble(operand2.Value);
                return CommonArithmeticLogic(operand1, operand2, value);
            }
            catch
            {
                return new UnknownConstant();
            }
        }

        public static IConstant ArithmeticDiv(IConstant operand1, IConstant operand2)
        {
            try
            {
                var value = (double)operand1.Value / (double)operand2.Value;
                return CommonArithmeticLogic(operand1, operand2, value);
            }
            catch
            {
                return new UnknownConstant();
            }
        }

        public static IConstant ArithmeticMod(IConstant operand1, IConstant operand2)
        {
            try
            {
                var value = (double)operand1.Value % (double)operand2.Value;
                return CommonArithmeticLogic(operand1, operand2, value);
            }
            catch
            {
                return new UnknownConstant();
            }
        }

        public static IConstant ArithmeticMul(IConstant operand1, IConstant operand2)
        {
            try
            {
                double value = (double)operand1.Value * (double)operand2.Value;
                return CommonArithmeticLogic(operand1, operand2, value);
            }
            catch
            {
                return new UnknownConstant();
            }
        }

        public static IConstant ArithmeticSub(IConstant operand1, IConstant operand2)
        {
            try
            {
                double value = (double)operand1.Value - (double)operand2.Value;
                return CommonArithmeticLogic(operand1, operand2, value);
            }
            catch
            {
                return new UnknownConstant();
            }
        }


        private static IConstant CommonArithmeticLogic(IConstant operand1, IConstant operand2, object valueCalculated)
        {
            try
            {
                // process by condition
                bool condition = (operand2 is IArithmetic);

                if (operand2 is IDouble) return new DoubleConstant((double)valueCalculated);
                if (condition) return new IntConstant(Convert.ToInt32(valueCalculated));

                return new UnknownConstant();
            }
            catch
            {
                return new UnknownConstant();
            }
        }
        #endregion



        #region The operation realted to Integer kind private.
        public static IConstant IntegerKindBitAnd(IConstant operand1, IConstant operand2)
        {
            try
            {
                int value = (int)operand1.Value & (int)operand2.Value;
                return CommonArithmeticLogic(operand1, operand2, value);
            }
            catch
            {
                return new UnknownConstant();
            }
        }

        public static IConstant IntegerKindBitNot(IConstant operand) => new IntConstant(~(int)operand.Value);

        public static IConstant IntegerKindBitOr(IConstant operand1, IConstant operand2)
        {
            try
            {
                var value = (int)operand1.Value | (int)operand2.Value;
                return CommonArithmeticLogic(operand1, operand2, value);
            }
            catch
            {
                return new UnknownConstant();
            }
        }

        public static IConstant IntegerKindBitXor(IConstant operand1, IConstant operand2)
        {
            try
            {
                var value = (int)operand1.Value ^ (int)operand2.Value;
                return CommonArithmeticLogic(operand1, operand2, value);
            }
            catch
            {
                return new UnknownConstant();
            }
        }

        public static IConstant IntegerKindLeftShift(IConstant operand, int count)
        {
            try
            {
                int tempValue = (int)operand.Value << count;

                return new IntConstant(tempValue);
            }
            catch
            {
                return new UnknownConstant();
            }
        }

        public static IConstant IntegerKindRightShift(IConstant operand, int count)
        {
            try
            {
                int tempValue = (int)operand.Value >> count;

                return new IntConstant(tempValue);
            }
            catch
            {
                return new UnknownConstant();
            }
        }
        #endregion



        #region The operation related to string type
        public static IConstant StringEqual(IConstant operand1, IConstant operand2)
        {
            try
            {
                if (operand2 is IString)
                {
                    string targetValue = operand2.Value as string;
                    return new BitConstant((string)operand1.Value == targetValue);
                }

                return new UnknownConstant();
            }
            catch
            {
                return new UnknownConstant();
            }
        }

        public static IConstant StringNotEqual(IConstant operand1, IConstant operand2)
        {
            try
            {
                if (operand2 is IString)
                {
                    string targetValue = operand2.Value as string;
                    return new BitConstant((string)operand1.Value != targetValue);
                }

                return new UnknownConstant();
            }
            catch
            {
                return new UnknownConstant();
            }
        }


        public static IConstant StringAdd(IConstant operand1, IConstant operand2)
        {
            try
            {
                if (operand2 is IString)
                {
                    string targetValue = operand1.Value + (operand2.Value as string);

                    return new StringConstant(targetValue);
                }

                if (operand2 is IInt)
                {
                    string targetValue = operand1.Value + (operand2.Value.ToString());

                    return new StringConstant(operand1.Value + targetValue);
                }

                return new UnknownConstant();
            }
            catch
            {
                return new UnknownConstant();
            }
        }
        #endregion
    }
}
