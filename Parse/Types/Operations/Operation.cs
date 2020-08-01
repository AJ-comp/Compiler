using Parse.Types.ConstantTypes;
using System;

namespace Parse.Types.Operations
{
    public class Operation
    {
        public static bool CanOperation(IValue operand1, IValue operand2)
        {
            if (operand1.PointerLevel != operand2.PointerLevel) return false;
            if (operand1.IsPointerType)
            {
                if (operand1.GetType() != operand2.GetType()) return false;
            }

            return true;
        }


        #region The operation related to Bit type
        public static IConstant BitTypeAnd(IValue operand1, IValue operand2)
        {
            if (!(operand2 is ILogicalOperation)) throw new FormatException();
            if (!Operation.CanOperation(operand1, operand2)) throw new FormatException();

            return new BitConstant((bool)operand1.Value && (bool)operand2.Value);
        }

        public static IConstant BitTypeNot(IValue operand) => new BitConstant(!(bool)operand.Value);

        public static IConstant BitTypeBitAnd(IValue operand1, IValue operand2)
        {
            if (!(operand2 is ILogicalOperation)) throw new FormatException();
            if (!Operation.CanOperation(operand1, operand2)) throw new FormatException();

            bool tempValue = (bool)operand1.Value & (bool)operand2.Value;

            return new BitConstant(tempValue);
        }

        public static IConstant BitTypeBitNot(IValue operand) => throw new NotSupportedException();

        public static IConstant BitTypeBitOr(IValue operand1, IValue operand2)
        {
            if (!Operation.CanOperation(operand1, operand2)) throw new FormatException();

            bool tempValue = (bool)operand1.Value | (bool)operand2.Value;

            return new BitConstant(tempValue);
        }

        public static IConstant BitTypeBitXor(IValue operand1, IValue operand2)
        {
            if (!Operation.CanOperation(operand1, operand2)) throw new FormatException();

            bool tempValue = (bool)operand1.Value ^ (bool)operand2.Value;

            return new BitConstant(tempValue);
        }

        public static IConstant BitTypeEqual(IValue operand1, IValue operand2)
        {
            if (!Operation.CanOperation(operand1, operand2)) throw new FormatException();

            return new BitConstant(operand1.Value == operand2.Value &&
                                              operand1.IsInitialized == operand2.IsInitialized);
        }

        public static IConstant BitTypeNotEqual(IValue operand1, IValue operand2)
        {
            if (!Operation.CanOperation(operand1, operand2)) throw new FormatException();

            throw new NotImplementedException();
        }

        public static IConstant BitTypeOr(IValue operand1, IValue operand2)
        {
            if (!Operation.CanOperation(operand1, operand2)) throw new FormatException();

            if (!(operand2 is ILogicalOperation)) throw new FormatException();

            return new BitConstant((bool)operand1.Value || (bool)operand2.Value);
        }

        public static IConstant BitTypeLeftShift(IValue operand, int count) => throw new NotSupportedException();
        public static IConstant BitTypeRightShift(IValue operand, int count) => throw new NotSupportedException();
        #endregion



        #region The operation realted to Arithmetic type
        public static IConstant ArithmeticEqual(IValue operand1, IValue operand2)
        {
            if (!(operand2 is ICompareOperation)) throw new NotSupportedException();
            if (operand2 is IString) throw new NotSupportedException();
            if (!CanOperation(operand1, operand2)) throw new FormatException();

            bool condition = (operand2 is IArithmetic);

            if (condition) return new BitConstant(operand1.PointerLevel, (int)operand1.Value == (int)operand2.Value);
            if (operand2 is IDouble) return new BitConstant((double)operand1.Value == (double)operand2.Value);

            // impossible
            throw new FormatException();
        }

        public static IConstant ArithmeticNotEqual(IValue operand1, IValue operand2)
        {
            if (!(operand2 is ICompareOperation)) throw new NotSupportedException();
            if (operand2 is IString) throw new NotSupportedException();
            if (!CanOperation(operand1, operand2)) throw new FormatException();

            bool condition = (operand2 is IArithmetic);

            if (condition) return new BitConstant((int)operand1.Value != (int)operand2.Value);
            if (operand2 is IDouble) return new BitConstant((double)operand1.Value != (double)operand2.Value);

            // impossible
            throw new FormatException();
        }


        public static IConstant ArithmeticAdd(IValue operand1, IValue operand2)
        {
            if (!(operand2 is IArithmeticOperation)) throw new NotSupportedException();
            if (!CanOperation(operand1, operand2)) throw new FormatException();

            // case string
            if (operand2 is IString)
            {
                var strValue = operand1.Value.ToString() + operand2.Value.ToString();

                return new StringConstant(strValue);
            }

            double value = Convert.ToDouble(operand1.Value) + Convert.ToDouble(operand2.Value);
            return CommonArithmeticLogic(operand1, operand2, value);
        }

        public static IConstant ArithmeticDiv(IValue operand1, IValue operand2)
        {
            if (!(operand2 is IArithmeticOperation)) throw new NotSupportedException();
            if (operand2 is IString) throw new NotSupportedException();
            if (!CanOperation(operand1, operand2)) throw new FormatException();

            var value = (double)operand1.Value / (double)operand2.Value;
            return CommonArithmeticLogic(operand1, operand2, value);
        }

        public static IConstant ArithmeticMod(IValue operand1, IValue operand2)
        {
            if (!(operand2 is IArithmeticOperation)) throw new NotSupportedException();
            if (operand2 is IString) throw new NotSupportedException();
            if (!CanOperation(operand1, operand2)) throw new FormatException();

            var value = (double)operand1.Value % (double)operand2.Value;
            return CommonArithmeticLogic(operand1, operand2, value);
        }

        public static IConstant ArithmeticMul(IValue operand1, IValue operand2)
        {
            if (!(operand2 is IArithmeticOperation)) throw new NotSupportedException();
            if (operand2 is IString) throw new NotSupportedException();
            if (!CanOperation(operand1, operand2)) throw new FormatException();

            double value = (double)operand1.Value * (double)operand2.Value;
            return CommonArithmeticLogic(operand1, operand2, value);
        }

        public static IConstant ArithmeticSub(IValue operand1, IValue operand2)
        {
            if (!(operand2 is IArithmeticOperation)) throw new NotSupportedException();
            if (operand2 is IString) throw new NotSupportedException();
            if (!CanOperation(operand1, operand2)) throw new FormatException();

            double value = (double)operand1.Value - (double)operand2.Value;
            return CommonArithmeticLogic(operand1, operand2, value);
        }


        private static IConstant CommonArithmeticLogic(IValue operand1, IValue operand2, object valueCalculated)
        {
            // process by condition
            bool condition = (operand2 is IArithmetic);

            if (operand2 is IDouble) return new DoubleConstant(operand1.PointerLevel, (double)valueCalculated);
            if (condition) return new IntConstant(operand1.PointerLevel, Convert.ToInt32(valueCalculated));

            // impossible
            throw new FormatException();
        }
        #endregion



        #region The operation realted to Integer kind.
        private static bool CheckValidity(IValue operand1, IValue operand2)
        {
            if (!CanOperation(operand1, operand2)) throw new FormatException();
            if (!(operand2 is IBitwiseOperation)) throw new NotSupportedException();
            if (operand2 is IDouble) throw new NotSupportedException();

            return true;
        }

        public static IConstant IntegerKindBitAnd(IValue operand1, IValue operand2)
        {
            CheckValidity(operand1, operand2);

            int value = (int)operand1.Value & (int)operand2.Value;
            return CommonArithmeticLogic(operand1, operand2, value);
        }

        public static IConstant IntegerKindBitNot(IValue operand) => new IntConstant(~(int)operand.Value);

        public static IConstant IntegerKindBitOr(IValue operand1, IValue operand2)
        {
            CheckValidity(operand1, operand2);

            var value = (int)operand1.Value | (int)operand2.Value;
            return CommonArithmeticLogic(operand1, operand2, value);
        }

        public static IConstant IntegerKindBitXor(IValue operand1, IValue operand2)
        {
            CheckValidity(operand1, operand2);

            var value = (int)operand1.Value ^ (int)operand2.Value;
            return CommonArithmeticLogic(operand1, operand2, value);
        }

        public static IConstant IntegerKindLeftShift(IValue operand, int count)
        {
            int tempValue = (int)operand.Value << count;

            return new IntConstant(tempValue);
        }

        public static IConstant IntegerKindRightShift(IValue operand, int count)
        {
            int tempValue = (int)operand.Value >> count;

            return new IntConstant(tempValue);
        }
        #endregion



        #region The operation related to string type
        public static IConstant StringEqual(IValue operand1, IValue operand2)
        {
            if (operand1.PointerLevel != operand2.PointerLevel) throw new FormatException();

            if (operand2 is IString)
            {
                string targetValue = operand2.Value as string;
                return new BitConstant((string)operand1.Value == targetValue);
            }
            else throw new NotSupportedException();
        }

        public static IConstant StringNotEqual(IValue operand1, IValue operand2)
        {
            if (operand1.PointerLevel != operand2.PointerLevel) throw new FormatException();

            if (operand2 is IString)
            {
                string targetValue = operand2.Value as string;
                return new BitConstant((string)operand1.Value != targetValue);
            }
            else throw new NotSupportedException();
        }


        public static IConstant StringAdd(IValue operand1, IValue operand2)
        {
            if (operand1.PointerLevel != operand2.PointerLevel) throw new FormatException();

            if (operand2 is IString)
            {
                string targetValue = operand1.Value + (operand2.Value as string);

                return new StringConstant(operand1.PointerLevel, targetValue);
            }

            if (operand2 is IInt)
            {
                string targetValue = operand1.Value + (operand2.Value.ToString());

                return new StringConstant(operand1.PointerLevel, operand1.Value + targetValue);
            }

            else throw new NotSupportedException();
        }
        #endregion
    }
}
