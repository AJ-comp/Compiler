using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using System.Diagnostics;

namespace Parse.FrontEnd.AJ.Sdts.Expressions
{
    /// **********************************************/
    /// <summary>
    /// 유저 정의 타입의 변수 선언문을 의미합니다.
    /// ex : A a = new A();
    /// </summary>
    /// **********************************************/
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class UserDefVarExpression : VariableExpression, IRDeclareUserTypeVar
    {
        public string TypeName { get; }

        public UserDefVarExpression(IDeclareVarExpression var) : base(var)
        {
            TypeName = var.TypeName;
        }

        private string GetDebuggerDisplay()
            => (PartyName.Length > 0) ? $"{TypeName} {PartyName}.{Name}"
                                                     : $"{TypeName} {Name}";
    }


    public class ThisVarExpression : VariableExpression
    {
//        public string TypeName { get; }

        public ThisVarExpression(StdType typeKind, string partyName, int length, string name)
            : base(typeKind, partyName, length, name)
        {
        }
    }
}
