using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types;
using Parse.Types.ConstantTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ
{
    public class AJOperator
    {
        public static IConstant ArithimeticOperation(IConstant left, IConstant right, IROperation operation, Action action = null)
        {
            // unknown type과의 연산은 확정할 수 없으므로 보류합니다. (아래와 같은 이유로 오류도 표시하지 않음)
            // unknown type이 있다는 것은 어딘가에 unknown type을 유도하는 오류 로직이 있다는 것이며
            // unknown type을 유도하는 첫번째 로직에서만 오류를 표시합니다.
            if (left is UnknownConstant || right is UnknownConstant) return new UnknownConstant();

            IConstant result = null;

            if (left is IArithmetic)
            {
                var cLeft = (left as IArithmetic);

                if (operation == IROperation.Add) result = cLeft.Add(right);
                else if (operation == IROperation.Sub) result = cLeft.Sub(right);
                else if (operation == IROperation.Mul) result = cLeft.Mul(right);
                else if (operation == IROperation.Div) result = cLeft.Div(right);
                else if (operation == IROperation.Mod) result = cLeft.Mod(right);
                else result = new UnknownConstant();
            }
            else if (left is IString)
            {
                var cLeft = (left as IArithmetic);

                if (operation == IROperation.Add) result = cLeft.Add(right);
                else result = new UnknownConstant();
            }
            else
            {
                // 확정된 타입이지만 연산이 불가한 경우
                action?.Invoke();
                result = new UnknownConstant();
            }

            return result;
        }
    }
}
