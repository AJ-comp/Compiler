using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes;
using Parse.MiddleEnd.IR;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types;
using Parse.Types.ConstantTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace Parse.FrontEnd.MiniC.Sdts.Datas.Variables
{
    public enum MiniCDataType
    {
        [Description("UnKnown")] Unknown,
        [Description("address")] Address,
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


    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public abstract class VariableMiniC : IDeclareVarExpression, ISymbolData
    {
        protected VariableMiniC(Access accessType, 
                                            MiniCTypeInfo typeDatas, TokenData nameToken, 
                                            TokenData levelToken, TokenData dimensionToken, 
                                            int blockLevel, int offset, IExprExpression initialExpr)
        {
            AccessType = accessType;
            ConstToken = typeDatas.ConstToken;
            DataTypeToken = typeDatas.DataTypeToken;
            NameToken = nameToken;
            LevelToken = levelToken;
            DimensionToken = dimensionToken;
            Block = blockLevel;
            Offset = offset;

            if(initialExpr == null) InitialExpr = new DefaultExprData(MiniCTypeConverter.ToStdDataType(DataType));
            else InitialExpr = initialExpr;

            IsVirtual = false;
        }

        public bool IsStatic { get; }
        public bool IsConst => ConstToken != null;
        public string TypeName { get; set; }


        public bool IsMatchWithVarName(string name) => (Name == name);
        public bool IsVirtual { get; }

        public MiniCDataType DataType => MiniCTypeConverter.ToMiniCDataType(DataTypeToken?.Input);
        public int Dimension => System.Convert.ToInt32(DimensionToken?.Input);


        public Access AccessType { get; internal set; }
        public TokenData ConstToken { get; }
        public TokenData DataTypeToken { get; }
        public TokenData NameToken { get; }
        public TokenData LevelToken { get; }
        public TokenData DimensionToken { get; }

        public string PartyName { get; set; }
        public string Name => NameToken?.Input;
        public int Block { get; set; }
        public int Offset { get; set; }
        public int Length => System.Convert.ToInt32(LevelToken?.Input);

        public uint PointerLevel { get; set; }
        public List<SdtsNode> ReferenceTable { get; } = new List<SdtsNode>();

        public abstract StdType TypeKind { get; }
        public IExprExpression InitialExpr { get; set; }
        public IConstant Result
        {
            get => (InitialExpr == null) ? new UnknownConstant() : InitialExpr.Result;
            protected set => Result = value;
        }


        public virtual bool Assign(IConstant constant)
        {
            bool result = false;

            if (TypeKind == constant.TypeKind)
            {
                result = true;
                Result = constant;
            }

            return result;
        }

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

        public override bool Equals(object obj)
        {
            return obj is VariableMiniC c &&
                   Name == c.Name &&
                   Block == c.Block &&
                   Offset == c.Offset;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Block, Offset);
        }

        public static bool operator ==(VariableMiniC left, VariableMiniC right)
        {
            return EqualityComparer<VariableMiniC>.Default.Equals(left, right);
        }

        public static bool operator !=(VariableMiniC left, VariableMiniC right)
        {
            return !(left == right);
        }


        private string DebuggerDisplay
                        => string.Format("{0}{1} {2}{3} Block: {4} Offset: {5} Length: {6}",
                                        (IsConst) ? "const " : string.Empty,
                                        Helper.GetEnumDescription(DataType),
                                        (PartyName.Length > 0) ? PartyName+"." : PartyName,
                                        Name,
                                        Block,
                                        Offset,
                                        Length);
    }




    public abstract class ValueVarMiniC : VariableMiniC
    {
        public ValueVarMiniC(Access accessType, MiniCTypeInfo typeDatas, TokenData nameToken,
                                        TokenData levelToken, TokenData dimensionToken,
                                        int blockLevel, int offset, IExprExpression value)
                                        : base(accessType, typeDatas, nameToken, levelToken, dimensionToken, blockLevel, offset, value)
        {
        }
    }


    public class PointerVariableMiniC : VariableMiniC
    {
        public PointerVariableMiniC(Access accessType, 
                                                MiniCTypeInfo typeDatas, 
                                                TokenData nameToken, 
                                                int blockLevel, int offset, uint pointerLevel, 
                                                ExprNode value, 
                                                StdType typeName)
                                                : base(accessType, typeDatas, nameToken, null, null, blockLevel, offset, value)
        {
            PointerLevel = pointerLevel;
            TypeKind = typeName;
        }

        public override StdType TypeKind { get; }

        public override bool Assign(IConstant constant)
        {
            if(constant is PointerConstant)
            {
                var pConstant = constant as PointerConstant;
                return (TypeKind == pConstant.TypeKind && PointerLevel == pConstant.PointerLevel);
            }

            return false;
        }
    }
}
