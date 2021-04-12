using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.MiddleEnd.IR.Datas;
using Parse.Types;

namespace Parse.FrontEnd.MiniC.Sdts.Expressions
{
    /// **********************************************/
    /// <summary>
    /// 유저 정의 타입의 변수 선언문을 의미합니다.
    /// ex : A a = new A();
    /// </summary>
    /// **********************************************/
    public class UserDefVarExpression : VariableExpression, IRDeclareUserTypeVar
    {
        public string TypeName { get; }

        public UserDefVarExpression(IDeclareVarExpression var) : base(var)
        {
            TypeName = var.TypeName;
        }
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
