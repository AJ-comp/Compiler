using AJ.Common.Helpers;
using Parse.Extensions;
using Parse.FrontEnd.AJ.Sdts.AstNodes;
using Parse.MiddleEnd.IR.Expressions;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Parse.FrontEnd.AJ.Data
{
    public enum VarProperty
    {
        Global = 0x01,
        Extern = 0x02,
        Param = 0x04,
        Readonly = 0x04,

    }


    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public partial class VariableAJ : ISymbolData, IHasType
    {
        public int Id { get; set; }
        public Access AccessType { get; internal set; }
        public AJTypeInfo Type { get; set; }
        public bool Param { get; set; } = false;
        public bool ReadOnly { get; set; } = false;
        public TokenData NameToken { get; set; }
        public int Block { get; set; }
        public int Offset { get; set; }
        public List<TokenData> LevelTokens { get; set; } = new List<TokenData>();
        public uint PointerLevel { get; set; }
        public uint Size { get; }
        public Initial InitValue { get; set; }
        public bool IsVirtual { get; set; }

        // The node that created own.
        public AJNode CreatedNode { get; set; }
        public List<AJNode> Reference { get; } = new List<AJNode>();

        public bool IsInitialized
        {
            get
            {
                if (InitValue == null) return false;

                if (InitValue.Value is VariableAJ)
                {
                    return (InitValue.Value as VariableAJ).IsInitialized;
                }

                return true;
            }
        }

        public List<TokenData> ReferenceTokens
        {
            get
            {
                List<TokenData> result = new List<TokenData>();

                foreach (var refer in Reference)
                {
                    foreach (var item in refer.Items)
                    {
                        if (!(item is TerminalNode)) continue;

                        var terminal = item as TerminalNode;
                        if (terminal.Token == NameToken) result.Add(terminal.Token);
                    }
                }

                return result;
            }
        }

        public ConstantAJ TerminalValue => InitValue?.TerminalValue;



        public VariableAJ(Access accessType,
                                        AJTypeInfo typeInfo, TokenData nameToken, IEnumerable<TokenData> levelTokens,
                                        int blockLevel, int offset)
        {
            AccessType = accessType;
            Type = typeInfo;
            NameToken = nameToken;
            _levelTokens.AddRangeExceptNull(levelTokens);
            Block = blockLevel;
            Offset = offset;

            IsVirtual = false;
        }

        public bool IsMatchWithVarName(string name) => (Name == name);
        public int Dimension => _levelTokens.Count;
        public AJDataType DataType => Type.DataType;

        public TokenData DataTypeToken { get; }
        public string Name => NameToken?.Input;


        public ConstantAJ ToConstantAJ()
        {
            //            return Type.Const ? new ConstantAJ() AJTypeConverter.To(Type.ToTypeInfo(), TerminalValue.Value, TerminalValue.ValueState)
            //                                       : AJTypeConverter.To(Type.ToTypeInfo(), null, State.Unknown);

            if (Type.Const) return TerminalValue;
            else return ConstantAJ.CreateValueUnknown(DataType);
        }


        public IRVariable ToIR()
        {
            IRVariable result = new IRVariable(Type.ToIR(), Name);

            result.BlockIndex = Block;
            result.OffsetIndex = Offset;

            return result;
        }

        public static VariableAJ CreateThisVar(AJDataType type, TokenData nameToken, int blockLevel, int offset)
        {
            return new VariableAJ(Access.Private, 
                                             AJTypeInfo.CreateThisType(type, nameToken), 
                                             TokenData.CreateStubToken(AJGrammar.Ident, "this"), 
                                             null, 
                                             blockLevel, 
                                             offset);
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
            return obj is VariableAJ c &&
                   Name == c.Name &&
                   Block == c.Block &&
                   Offset == c.Offset;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Block, Offset);
        }

        public static bool operator ==(VariableAJ left, VariableAJ right)
        {
            return EqualityComparer<VariableAJ>.Default.Equals(left, right);
        }

        public static bool operator !=(VariableAJ left, VariableAJ right)
        {
            return !(left == right);
        }



        private List<TokenData> _levelTokens = new List<TokenData>();

        private string DebuggerDisplay => $"{AccessType} {Type.GetDebuggerDisplay()} {Name} (Block: {Block} Offset: {Offset})";
    }




    public abstract class ValueVarAJ : VariableAJ
    {
        public ValueVarAJ(Access accessType,
                                        AJTypeInfo typeInfo, TokenData nameToken,
                                        IEnumerable<TokenData> levelTokens,
                                        int blockLevel, int offset)
                                        : base(accessType, typeInfo, nameToken, levelTokens, blockLevel, offset)
        {
        }
    }


    /*
    public class PointerVariableAJ : VariableAJ
    {
        public PointerVariableAJ(Access accessType,
                                                AJTypeInfo typeInfo,
                                                TokenData nameToken,
                                                int blockLevel, int offset, uint pointerLevel)
                                                : base(accessType, typeInfo, nameToken, null, blockLevel, offset)
        {
            PointerLevel = pointerLevel;
        }

        public override AJDataType DataType => AJDataType.Address;

        public override bool Assign(IConstant constant)
        {
            if (constant is PointerConstant)
            {
                var pConstant = constant as PointerConstant;
                return (TypeKind == pConstant.TypeKind && PointerLevel == pConstant.PointerLevel);
            }

            return false;
        }
    }
    */
}
