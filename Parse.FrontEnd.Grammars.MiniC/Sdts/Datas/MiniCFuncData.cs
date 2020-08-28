﻿using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables;
using System.Collections.Generic;
using static Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables.VariableMiniC;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.Datas
{
    public class MiniCFuncData
    {
        public MiniCDataType ReturnType => TypeData.DataType;
        public string Name => NameToken.Input;

        public MiniCTypeInfo TypeData { get; private set; }
        public TokenData NameToken { get; private set; }
        public int Offset { get; private set; }
        public List<VariableMiniC> ParamVars { get; } = new List<VariableMiniC>();

        public MiniCFuncData(MiniCTypeInfo typeData, TokenData nameToken, int offset, IEnumerable<VariableMiniC> paramVars)
        {
            TypeData = typeData;
            NameToken = nameToken;
            Offset = offset;
            ParamVars.AddRange(paramVars);
        }


        public string ToDefineString(bool bDisplayReturnType, bool bDisplayParams)
        {
            string result = (bDisplayReturnType) ? ReturnType.ToString() + Name : Name;

            if (bDisplayParams)
            {
                result += "(";
                foreach (var param in ParamVars)
                    result += param.DataType.ToString() + ",";

                result = result.Substring(0, result.Length - 1) + ")";
            }

            return result;
        }
    }
}