using Parse.Extensions;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.MiddleEnd.IR.Datas;
using System.Diagnostics;

namespace Parse.FrontEnd.AJ.Sdts.Expressions
{
    /// ***************************************************/
    /// <summary>
    /// 미리 정의된 변수 타입 선언문을 의미합니다.
    /// ex : int a = 10;
    /// </summary>
    /// ***************************************************/
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class PreDefVarExpression : VariableExpression, IRDeclareVar
    {
        public PreDefVarExpression(IDeclareVarExpression var) : base(var)
        {
        }

        private string GetDebuggerDisplay()
            => (PartyName.Length > 0) ? $"{TypeKind.ToDescription()} {PartyName}.{Name}"
                                                     : $"{TypeKind.ToDescription()} {Name}";
    }
}
