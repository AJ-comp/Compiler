using Parse.FrontEnd.MiniC.Sdts.Datas.Variables;

namespace Parse.FrontEnd.MiniC.Sdts.Datas
{
    public class MiniCTypeInfo
    {
        public MiniCTypeInfo(TokenData constToken, TokenData dataTypeToken, TokenData pointerToken = null)
        {
            ConstToken = constToken;
            DataTypeToken = dataTypeToken;
            PointerToken = pointerToken;
        }

        public bool Const => ConstToken != null;
        public MiniCDataType DataType => MiniCTypeConverter.ToMiniCDataType(DataTypeToken?.Input);

        public TokenData ConstToken { get; private set; }
        public TokenData DataTypeToken { get; private set; }
        public TokenData PointerToken { get; private set; }
    }
}
