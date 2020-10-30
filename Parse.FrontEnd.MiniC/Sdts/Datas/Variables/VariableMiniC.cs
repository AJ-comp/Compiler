using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using Parse.Types.ConstantTypes;
using Parse.Types.VarTypes;
using System;
using System.ComponentModel;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables
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

    public abstract class VariableMiniC : Variable, IRVar, IHasName
    {
        protected VariableMiniC(MiniCTypeInfo typeDatas, TokenData nameToken,
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

        public bool IsMatchWithVarName(string name) => (Name == name);
        public bool IsVirtual { get; }

        public bool Const => ConstToken != null;
        public MiniCDataType DataType => MiniCTypeConverter.ToMiniCDataType(DataTypeToken?.Input);
        public int Dimension => System.Convert.ToInt32(DimensionToken?.Input);


        public TokenData ConstToken { get; }
        public TokenData DataTypeToken { get; }
        public TokenData NameToken { get; }
        public TokenData LevelToken { get; }
        public TokenData DimensionToken { get; }

        public VarProperty VariableProperty { get; }

        public string Name => NameToken?.Input;
        public int Block { get; set; }
        public int Offset { get; set; }
        public int Length => System.Convert.ToInt32(LevelToken?.Input);

        public uint PointerLevel { get; set; }

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



        public static IValue Convert(VarProperty varProperty, ExprNode node)
        {
            if (node is UseIdentNode) return (node as UseIdentNode).VarData;
            else if (node is LiteralNode) return (node as LiteralNode).Result;
            // Only global variable uses ExprNode.Result.
            else if (varProperty == VarProperty.Global && node?.Result != null) return node.Result;

            return new IntConstant(0, State.NotInit);
        }
    }

    public abstract class ValueVarMiniC : VariableMiniC
    {
        public ValueVarMiniC(MiniCTypeInfo typeDatas, TokenData nameToken,
                                        TokenData levelToken, TokenData dimensionToken,
                                        int blockLevel, int offset, VarProperty varProperty, IValue value)
                                        : base(typeDatas, nameToken, levelToken, dimensionToken, blockLevel, offset, varProperty, value)
        {
        }


        public override IConstant Assign(IValue operand)
        {
            throw new NotImplementedException();
        }
    }




    public class PointerVariableMiniC : VariableMiniC, IRVar
    {
        public PointerVariableMiniC(MiniCTypeInfo typeDatas, TokenData nameToken, int offset, VarProperty varProperty, uint pointerLevel, ExprNode value, DType typeName)
                                                : base(typeDatas, nameToken, null, null, 0, offset, varProperty, VariableMiniC.Convert(varProperty, value))
        {
            PointerLevel = pointerLevel;
            TypeName = typeName;
        }

        public override DType TypeName { get; }

        public override bool CanAssign(IValue operand)
        {
            if (operand is IntConstant) return true;

            if (operand is PointerVariableMiniC)
            {
                var variable = (operand as PointerVariableMiniC);
                return (PointerLevel == variable.PointerLevel);
            }

            return false;
        }

        public override IConstant Assign(IValue operand)
        {
            if (!CanAssign(operand)) throw new NotSupportedException();

            if (operand is PointerVariableMiniC)
            {
                var variable = (operand as PointerVariable);
                var valueConstant = variable.ValueConstant;

                // operand may be not int type so it has to make int type explicity.
                ValueConstant = new IntConstant((int)valueConstant.Value,
                                                                    valueConstant.ValueState);
            }
            else
            {
                var valueConstant = (operand as IConstant);

                // operand may be not int type so it has to make int type explicity.
                ValueConstant = new IntConstant((int)valueConstant.Value,
                                                                    valueConstant.ValueState);
            }

            return ValueConstant;
        }
    }
}
