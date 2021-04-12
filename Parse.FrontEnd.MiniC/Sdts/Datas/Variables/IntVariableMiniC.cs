using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes;
using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using Parse.Types.ConstantTypes;
using Parse.Types.VarTypes;

namespace Parse.FrontEnd.MiniC.Sdts.Datas.Variables
{
    public class IntVariableMiniC : VariableMiniC
    {
        public bool Signed { get; }
        public int Size => 32;
        public override StdType TypeKind => Signed ? StdType.UInt : StdType.Int;

        public IntVariableMiniC(Access accessType,
                                            MiniCTypeInfo typeDatas,
                                            TokenData nameToken,
                                            TokenData levelToken,
                                            TokenData dimensionToken,
                                            int blockLevel,
                                            int offset,
                                            ExprNode value)
            : base(accessType, typeDatas, nameToken, levelToken, dimensionToken, blockLevel, offset, value)
        {

        }

        public override bool Assign(IConstant constant)
        {
            // 타입이 같은 경우는 여기서 true로 빠져 나간다
            if (base.Assign(constant)) return true;

            bool result = false;
            if (constant is IDouble) return result;

            if (constant is IIntegerKind)
            {
                if ((constant as IIntegerKind).Signed == Signed) result = true;
            }

            return result;
        }
    }
}
