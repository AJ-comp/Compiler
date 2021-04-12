using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types;

namespace Parse.FrontEnd.MiniC.Sdts.Datas.Variables
{
    public abstract class UserDefVariableMiniC : VariableMiniC
    {
        public UserDefVariableMiniC(Access accessType, 
                                                MiniCTypeInfo typeDatas, 
                                                TokenData nameToken, 
                                                TokenData levelToken, 
                                                TokenData dimensionToken, 
                                                int blockLevel, 
                                                int offset, 
                                                IExprExpression value)
            : base(accessType, typeDatas, nameToken, levelToken, dimensionToken, blockLevel, offset, value)
        {
        }
    }
}
