using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.MiddleEnd.IR.Datas;

namespace Parse.FrontEnd.MiniC.Sdts.Expressions
{
    /// ***************************************************/
    /// <summary>
    /// 미리 정의된 변수 타입 선언문을 의미합니다.
    /// ex : int a = 10;
    /// </summary>
    /// ***************************************************/
    public class PreDefVarExpression : VariableExpression, IRDeclareVar
    {
        public PreDefVarExpression(IDeclareVarExpression var) : base(var)
        {
        }

        public override string ToString()
        {
            return string.Empty;
            //return string.Format("{0}{1}{2}{4} {5}",
            //                                Is)
        }
    }
}
