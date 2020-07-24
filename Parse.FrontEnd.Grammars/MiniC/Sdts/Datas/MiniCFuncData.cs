using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.Datas
{
    public class MiniCFuncData
    {
        public MiniCDataType ReturnType => TypeData.DataType;
        public string Name => NameToken.Input;

        public MiniCTypeInfo TypeData { get; private set; }
        public TokenData NameToken { get; private set; }
        public int Offset { get; private set; }
        public List<MiniCVarData> ParamVars { get; } = new List<MiniCVarData>();

        public MiniCFuncData(MiniCTypeInfo typeData, TokenData nameToken, int offset, IReadOnlyList<MiniCVarData> paramVars)
        {
            TypeData = typeData;
            NameToken = nameToken;
            Offset = offset;
            ParamVars.AddRange(paramVars);
        }
    }
}
