using Parse.FrontEnd.Grammars.Properties;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Datas.Types;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.Datas
{
    public enum MiniCDataType { Unknown, Void, Int }
    public enum EtcInfo { Normal, Extern, Param }

    public class MiniCVarData : IRVar
    {
        public MeaningErrInfoList MeaningErrorList
        {
            get
            {
                MeaningErrInfoList result = new MeaningErrInfoList();

                if (Value.IsUnknown)
                {
                    var convertedLhs = Value;

                    if (convertedLhs.IsOnlyNotInit)
                        result.Add(new MeaningErrInfo(nameof(AlarmCodes.MCL0005), string.Format(AlarmCodes.MCL0005, Name)));
                    else if (convertedLhs.IsNotInitAndDynamicAlloc)
                        result.Add(new MeaningErrInfo(nameof(AlarmCodes.MCL0005), string.Format(AlarmCodes.MCL0005, Name), ErrorType.Warning));
                }

                return result;
            }
        }

        public DType TypeName => MiniCTypeConverter.ToIRDataType(DataType);
        public string Name => NameToken?.Input;
        public int Block => _blockLevel;
        public int Offset => _offset;
        public int Length => System.Convert.ToInt32(LevelToken?.Input);
        public ValueData Value { get; set; }

        public bool Const => ConstToken != null;
        public MiniCDataType DataType => MiniCTypeConverter.ToMiniCDataType(DataTypeToken?.Input);
        public int Dimension => System.Convert.ToInt32(DimensionToken?.Input);
        public EtcInfo Etc { get; private set; }


        public TokenData ConstToken { get; private set; }
        public TokenData DataTypeToken { get; private set; }
        public TokenData NameToken { get; private set; }
        public TokenData LevelToken { get; private set; }
        public TokenData DimensionToken { get; private set; }


        public bool IsMatchWithVarName(string name) => (Name == name);
        public bool IsVirtual { get; private set; }



        public MiniCVarData (MiniCTypeInfo typeDatas, TokenData nameToken,
                                        TokenData levelToken, TokenData dimensionToken,
                                        int blockLevel, int offset, EtcInfo etc,
                                        ValueData value = null)
        {
            ConstToken = typeDatas.ConstToken;
            DataTypeToken = typeDatas.DataTypeToken;
            NameToken = nameToken;
            LevelToken = levelToken;
            DimensionToken = dimensionToken;
            _blockLevel = blockLevel;
            _offset = offset;
            Etc = etc;
            Value = value;

            IsVirtual = false;
        }

        public MiniCVarData(TokenData nameToken, int block, int offset)
        {
            NameToken = nameToken;
            _blockLevel = block;
            _offset = offset;

            IsVirtual = true;
        }




        public override string ToString()
                    => string.Format("const : {0}, DataType : {1}, Name : {2}, Level : {3}, Dimension : {4}, block level : {5}, offset : {6}, Etc : {7}",
                                                Const.ToString(), DataType.ToString(), Name, LevelToken.Input, Dimension, Block, Offset, Etc.ToString());

        private int _blockLevel;
        private int _offset;
    }
}
