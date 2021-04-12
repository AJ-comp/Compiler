using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.MiniC.Sdts.Expressions.ExprExpressions
{
    /// **********************************************/
    /// <summary>
    /// 전위, 후위 연산식을 의미하는 표현식입니다.
    /// 예를 들어 ProcessInfo 값이 PreInc 인 경우 : ++Var
    /// </summary>
    /// **********************************************/
    public class IncDecExpression : ExprExpression, IRIncDecExpr
    {
        public Info ProcessInfo { get; }
        public IRDeclareVar Var { get; }

        public IncDecExpression(IIncDecExpression node) : base(node)
        {
            ProcessInfo = node.ProcessInfo;
            
            Var = VariableExpression.Create(node.Var);
        }
    }
}
