using Parse.Types;

namespace Parse.FrontEnd.MiniC.Sdts.Datas.Variables
{
    public class EnumDefVariableMiniC : UserDefVariableMiniC
    {
        public EnumDefVariableMiniC(string typeName,
                                                    Access accessType, 
                                                    MiniCTypeInfo typeDatas, 
                                                    TokenData nameToken, 
                                                    TokenData levelToken, 
                                                    TokenData dimensionToken, 
                                                    int blockLevel, 
                                                    int offset, 
                                                    IExprExpression value)
            : base(accessType, typeDatas, nameToken, levelToken, dimensionToken, blockLevel, offset, value)
        {
            TypeName = typeName;
        }

        public override StdType TypeKind => StdType.Enum;
    }
}
