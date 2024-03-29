﻿using Parse.Types;
using Parse.Types.ConstantTypes;

namespace Parse.FrontEnd.AJ.Sdts.Datas
{
    public class DefaultExprData : IExprBuildNode
    {
        public IConstant Result { get; }

        public DefaultExprData(StdType type)
        {
            Result = new NotInitConstant(type);
        }
    }
}
