using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.AJ.Sdts.Expressions.ExprExpressions
{
    /// **********************************************/
    /// <summary>
    /// 논리 연산식(비교식 포함)을 의미하는 표현식입니다.
    /// 예를 들어 CompareSymbol 값이 EQ 인 경우 : Left == Right
    /// </summary>
    /// **********************************************/
    public class ConditionExpression : BinaryExpression, IRCompareOpExpr
    {
        public IRCompareSymbol CompareSymbol { get; }

        public IRExpr Left { get; }
        public IRExpr Right { get; }

        public ConditionExpression(ICompareExpression compareNode)
        {
            Left = Create(compareNode.Left);
            Right = Create(compareNode.Right);

            CompareSymbol = compareNode.CompareOper;
        }
    }
}
