using Parse.MiddleEnd.IR;
using Parse.MiddleEnd.IR.Datas.Types;
using Parse.MiddleEnd.IR.Datas.ValueDatas;
using System;
using System.Collections.Generic;
using Byte = Parse.MiddleEnd.IR.Datas.Types.Byte;

namespace Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat
{
    public abstract class ValueData : IRValue
    {
        private List<TokenData> _tokenList;

        protected ValueData(IReadOnlyList<TokenData> tokenList)
        {
            _tokenList.AddRange(tokenList);
        }

        public IReadOnlyList<TokenData> TokenList => _tokenList;

        public abstract bool IsZero { get; protected set; }
        public abstract DType TypeName { get; protected set; }
        public abstract object Value { get; protected set; }
        public abstract bool Signed { get; protected set; }
        public abstract bool IsNan { get; protected set; }

        public abstract IRValue Add(IRValue t);
        public abstract IRValue Div(IRValue t);
        public abstract bool? IsEqual(IRValue t);
        public abstract bool? IsGreaterThan(IRValue t);
        public abstract bool? IsLessThan(IRValue t);
        public abstract IRValue<Bit> LogicalOp(IRValue t, IRCondition cond);
        public abstract IRValue Mod(IRValue t);
        public abstract IRValue Mul(IRValue t);
        public abstract IRValue Sub(IRValue t);
    }


    public class ValueData<T> : ValueData, IRValue<T> where T : DataType
    {
        public ValueData(IReadOnlyList<TokenData> tokenList, object value) : base(tokenList)
        {
            Value = value;
        }

        public ValueData(IReadOnlyList<TokenData> tokenList, bool isNan) : base(tokenList)
        {
            IsNan = isNan;
        }

        public override bool IsZero { get; protected set; }
        public override DType TypeName { get; protected set; }
        public override object Value { get; protected set; }
        public override bool Signed { get; protected set; }
        public override bool IsNan { get; protected set; }

        public override IRValue Add(IRValue t)
        {
            return CommonOp(t, (() => (double)Value + (double)t.Value), 
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
                    return new ValueData<DoubleType>(TokenList, true);
                else if (greaterType == DType.Int)
                    return new ValueData<Int>(TokenList, true);
                else if (greaterType == DType.Short)
                    return new ValueData<Short>(TokenList, true);
                else if (greaterType == DType.Byte)
                    return new ValueData<Byte>(TokenList, true);

                return new ValueData<Bit>(TokenList, true);
            }

            return CommonOp(t, () => (double)Value / (double)t.Value,
                                            () => (int)Value / (int)t.Value,
                                            () => (short)((short)Value / (short)t.Value),
                                            () => (byte)((byte)Value / (byte)t.Value),
                                            () => (byte)((byte)Value / (byte)t.Value));
        }

        public override bool? IsEqual(IRValue t)
        {
            if (TypeName != t.TypeName) return false;
            if (Value != t.Value) return false;

            return true;
        }

        public override bool? IsGreaterThan(IRValue t)
        {
            throw new System.NotImplementedException();
        }

        public override bool? IsLessThan(IRValue t)
        {
            throw new System.NotImplementedException();
        }

        public override IRValue<Bit> LogicalOp(IRValue t, IRCondition cond)
        {
            throw new System.NotImplementedException();
        }

        public override IRValue Mod(IRValue t)
        {
            if ((double)t.Value == 0)
            {
                DType greaterType = IRChecker.GetGreaterType(this.TypeName, t.TypeName);

                if (greaterType == DType.Double)
                    return new ValueData<DoubleType>(TokenList, true);
                else if (greaterType == DType.Int)
                    return new ValueData<Int>(TokenList, true);
                else if (greaterType == DType.Short)
                    return new ValueData<Short>(TokenList, true);
                else if (greaterType == DType.Byte)
                    return new ValueData<Byte>(TokenList, true);

                return new ValueData<Bit>(TokenList, true);
            }

            return CommonOp(t, () => (double)Value % (double)t.Value,
                                            () => (int)Value % (int)t.Value,
                                            () => (short)((short)Value % (short)t.Value),
                                            () => (byte)((byte)Value % (byte)t.Value),
                                            () => (byte)((byte)Value % (byte)t.Value));
        }

        public override IRValue Mul(IRValue t)
        {
            return CommonOp(t, (() => (double)Value * (double)t.Value),
                                            (() => (int)Value * (int)t.Value),
                                            (() => (short)((short)Value * (short)t.Value)),
                                            (() => (byte)((byte)Value * (byte)t.Value)),
                                            (() => (byte)((byte)Value * (byte)t.Value)));
        }

        public override IRValue Sub(IRValue t)
        {
            return CommonOp(t, (() => (double)Value - (double)t.Value),
                                            (() => (int)Value - (int)t.Value),
                                            (() => (short)((short)Value - (short)t.Value)),
                                            (() => (byte)((byte)Value - (byte)t.Value)),
                                            (() => (byte)((byte)Value - (byte)t.Value)));
        }

        private IRValue CommonOp(IRValue t, Func<double> doubleLogic, Func<int> intLogic, 
                                                                Func<short> shortLogic, Func<byte> byteLogic, 
                                                                Func<byte> bitLogic)
        {
            DType greaterType = IRChecker.GetGreaterType(this.TypeName, t.TypeName);

            if (greaterType == DType.Double)
            {
                ValueData<DoubleType> result = new ValueData<DoubleType>(TokenList, doubleLogic.Invoke());
                return result;
            }
            else if (greaterType == DType.Int)
            {
                ValueData<Int> result = new ValueData<Int>(TokenList, intLogic.Invoke());
                return result;
            }
            else if (greaterType == DType.Short)
            {
                ValueData<Short> result = new ValueData<Short>(TokenList, shortLogic.Invoke());
                return result;
            }
            else if (greaterType == DType.Byte)
            {
                ValueData<Byte> result = new ValueData<Byte>(TokenList, byteLogic.Invoke());
                return result;
            }
            else if (greaterType == DType.Bit)
            {
                ValueData<Bit> result = new ValueData<Bit>(TokenList, bitLogic.Invoke());
                return result;
            }

            return null;
        }
    }
}
