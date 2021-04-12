﻿using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes;
using Parse.Types;
using System;

namespace Parse.FrontEnd.MiniC.Sdts.Datas.Variables
{
    public class StringVariableMiniC : VariableMiniC
    {
        public StringVariableMiniC(Access accessType,
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

        public override StdType TypeKind => StdType.Unknown;
        public int Size => throw new NotImplementedException();
    }
}
