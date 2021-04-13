using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types;

namespace Parse.FrontEnd.AJ.Sdts.Datas.Variables
{
    public class StructVariableMiniC : UserDefVariableMiniC
    {
        public StructVariableMiniC(string typeName,
                                                uint biggeestMemberSize,
                                                Access accessType, 
                                                AJTypeInfo typeDatas, 
                                                TokenData nameToken, 
                                                TokenData levelToken, 
                                                TokenData dimensionToken, 
                                                int blockLevel, int offset, 
                                                IExprExpression value)
            : base(accessType, typeDatas, nameToken, levelToken, dimensionToken, blockLevel, offset, value)
        {
            TypeName = typeName;
            BiggestMemberSize = biggeestMemberSize;
        }

        public override StdType TypeKind => StdType.Struct;
        public uint BiggestMemberSize { get; }

        public IRStructDefInfo DefInfo { get; }
    }
}
