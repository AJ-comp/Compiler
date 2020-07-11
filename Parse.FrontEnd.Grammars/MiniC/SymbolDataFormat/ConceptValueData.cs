using Parse.MiddleEnd.IR;
using Parse.MiddleEnd.IR.Datas.Types;
using Parse.MiddleEnd.IR.Datas.ValueDatas;
using System.Collections.Generic;
using Byte = Parse.MiddleEnd.IR.Datas.Types.Byte;

namespace Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat
{
    public abstract class ConceptValueData : ValueData
    {
        protected ConceptValueData(IReadOnlyList<TokenData> tokenList)
        {
            _tokenList.AddRange(tokenList);
        }

        public IReadOnlyList<TokenData> TokenList => _tokenList;

        private List<TokenData> _tokenList = new List<TokenData>();
    }





    public class ConceptValueData<T> : ConceptValueData, IRValue<T> where T : DataType
    {
        public ConceptValueData(IReadOnlyList<TokenData> tokenList, object value) : base(tokenList)
        {
            Value = value;
        }

        public ConceptValueData(IReadOnlyList<TokenData> tokenList, bool isNan) : base(tokenList)
        {
            IsNan = isNan;
        }

        public override bool IsZero { get; }
        public override DType TypeName => DataType.GetTypeName(typeof(T));
        public override object Value { get; }
        public override bool Signed { get; }
        public override bool IsNan { get; }

        public override IRValue Add(IRValue t)
        {
            return CommonLogic.CommonOp(this, t, 
                                                            (() => (double)Value + (double)t.Value), 
                                                            (() => (int)Value + (int)t.Value), 
                                                            (() => (short)((short)Value + (short)t.Value)), 
                                                            (() => (byte)((byte)Value + (byte)t.Value)), 
                                                            (() => (byte)((byte)Value + (byte)t.Value)));
        }

        public override IRValue Div(IRValue t)
        {
            if ((double)t.Value == 0)
            {
                DType greaterType = IRChecker.GetGreaterType(this.TypeName, t.TypeName);

                if (greaterType == DType.Double)
                    return new ConceptValueData<DoubleType>(TokenList, true);
                else if (greaterType == DType.Int)
                    return new ConceptValueData<Int>(TokenList, true);
                else if (greaterType == DType.Short)
                    return new ConceptValueData<Short>(TokenList, true);
                else if (greaterType == DType.Byte)
                    return new ConceptValueData<Byte>(TokenList, true);

                return new ConceptValueData<Bit>(TokenList, true);
            }

            return CommonLogic.CommonOp(this, t, 
                                                            () => (double)Value / (double)t.Value,
                                                            () => (int)Value / (int)t.Value,
                                                            () => (short)((short)Value / (short)t.Value),
                                                            () => (byte)((byte)Value / (byte)t.Value),
                                                            () => (byte)((byte)Value / (byte)t.Value));
        }

        public override IRValue Mod(IRValue t)
        {
            if ((double)t.Value == 0)
            {
                DType greaterType = IRChecker.GetGreaterType(this.TypeName, t.TypeName);

                if (greaterType == DType.Double)
                    return new ConceptValueData<DoubleType>(TokenList, true);
                else if (greaterType == DType.Int)
                    return new ConceptValueData<Int>(TokenList, true);
                else if (greaterType == DType.Short)
                    return new ConceptValueData<Short>(TokenList, true);
                else if (greaterType == DType.Byte)
                    return new ConceptValueData<Byte>(TokenList, true);

                return new ConceptValueData<Bit>(TokenList, true);
            }

            return CommonLogic.CommonOp(this, t, () => (double)Value % (double)t.Value,
                                                            () => (int)Value % (int)t.Value,
                                                            () => (short)((short)Value % (short)t.Value),
                                                            () => (byte)((byte)Value % (byte)t.Value),
                                                            () => (byte)((byte)Value % (byte)t.Value));
        }

        public override IRValue Mul(IRValue t)
        {
            return CommonLogic.CommonOp(this, t, 
                                                            (() => (double)Value * (double)t.Value),
                                                            (() => (int)Value * (int)t.Value),
                                                            (() => (short)((short)Value * (short)t.Value)),
                                                            (() => (byte)((byte)Value * (byte)t.Value)),
                                                            (() => (byte)((byte)Value * (byte)t.Value)));
        }

        public override IRValue Sub(IRValue t)
        {
            return CommonLogic.CommonOp(this, t, 
                                                            (() => (double)Value - (double)t.Value),
                                                            (() => (int)Value - (int)t.Value),
                                                            (() => (short)((short)Value - (short)t.Value)),
                                                            (() => (byte)((byte)Value - (byte)t.Value)),
                                                            (() => (byte)((byte)Value - (byte)t.Value)));
        }
    }
}
