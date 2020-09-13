using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using Parse.Types.ConstantTypes;
using Parse.Types.VarTypes;
using System;
using System.ComponentModel;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables
{
    public abstract class VariableMiniC : Variable, IRVar, IHasName
    {
        public enum MiniCDataType
        {
            [Description("UnKnown")] Unknown,
            [Description("void")] Void,
            [Description("int")] Int,
            [Description("string")] String
        }
        public enum VarProperty
        {
            [Description("global")] Global,
            [Description("normal")] Normal,
            [Description("extern")] Extern,
            [Description("param")] Param
        }

        public VariableMiniC(MiniCTypeInfo typeDatas, TokenData nameToken,
                                        TokenData levelToken, TokenData dimensionToken,
                                        int blockLevel, int offset, VarProperty varProperty, IValue value) : base(value)
        {
            ConstToken = typeDatas.ConstToken;
            DataTypeToken = typeDatas.DataTypeToken;
            NameToken = nameToken;
            LevelToken = levelToken;
            DimensionToken = dimensionToken;
            Block = blockLevel;
            Offset = offset;
            VariableProperty = varProperty;

            IsVirtual = false;
        }

        public string Name => NameToken?.Input;
        public int Block { get; }
        public int Offset { get; }
        public int Length => System.Convert.ToInt32(LevelToken?.Input);

        public bool IsMatchWithVarName(string name) => (Name == name);
        public bool IsVirtual { get; }

        public bool Const => ConstToken != null;
        public MiniCDataType DataType => MiniCTypeConverter.ToMiniCDataType(DataTypeToken?.Input);
        public int Dimension => System.Convert.ToInt32(DimensionToken?.Input);
        public VarProperty VariableProperty { get; }


        public TokenData ConstToken { get; }
        public TokenData DataTypeToken { get; }
        public TokenData NameToken { get; }
        public TokenData LevelToken { get; }
        public TokenData DimensionToken { get; }

        //public MeaningErrInfoList MeaningErrorList
        //{
        //    get
        //    {
        //        MeaningErrInfoList result = new MeaningErrInfoList();

        //        if (Value.IsUnknown)
        //        {
        //            var convertedLhs = Value;

        //            if (convertedLhs.IsOnlyNotInit)
        //                result.Add(new MeaningErrInfo(nameof(AlarmCodes.MCL0005), string.Format(AlarmCodes.MCL0005, Name)));
        //            else if (convertedLhs.IsNotInitAndDynamicAlloc)
        //                result.Add(new MeaningErrInfo(nameof(AlarmCodes.MCL0005), string.Format(AlarmCodes.MCL0005, Name), ErrorType.Warning));
        //        }

        //        return result;
        //    }
        //}


        public override IConstant Assign(IValue operand)
        {
            throw new NotImplementedException();
        }
    }
}
