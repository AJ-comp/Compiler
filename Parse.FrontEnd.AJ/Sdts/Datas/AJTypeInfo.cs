using Parse.FrontEnd.AJ.Sdts.Datas.Variables;

namespace Parse.FrontEnd.AJ.Sdts.Datas
{
    public class AJTypeInfo
    {
        public AJTypeInfo(TokenData constToken, TokenData dataTypeToken, TokenData pointerToken = null)
        {
            ConstToken = constToken;
            DataTypeToken = dataTypeToken;
            PointerToken = pointerToken;
        }

        public bool Const => ConstToken != null;
        public MiniCDataType DataType => AJTypeConverter.ToMiniCDataType(DataTypeToken?.Input);

        public TokenData ConstToken { get; private set; }
        public TokenData DataTypeToken { get; private set; }
        public TokenData PointerToken { get; private set; }
    }
}
