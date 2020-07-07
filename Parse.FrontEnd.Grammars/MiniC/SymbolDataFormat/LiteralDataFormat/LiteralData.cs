using Parse.MiddleEnd.IR;
using Parse.MiddleEnd.IR.Datas.Types;
using Parse.MiddleEnd.IR.Datas.ValueDatas;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.LiteralDataFormat
{
    public abstract class LiteralData : IRValue, ICloneable
    {
        public string LiteralName
        {
            get
            {
                string result = string.Empty;
                foreach (var item in ValueToken) result += item.Input;

                return result;
            }
        }

        public IReadOnlyList<TokenData> ValueToken => _valueTokens;

        public bool IsVirtual { get; protected set; } = false;
        public abstract bool IsZero { get; }
        public abstract DType TypeName { get; }
        public abstract object Value { get; }
        public abstract bool Signed { get; }
        public abstract bool IsNan { get; }

        protected LiteralData(IReadOnlyList<TokenData> valueToken)
        {
            _valueTokens.AddRange(valueToken);
        }

        public abstract object Clone();
        public abstract IRValue<Bit> LogicalOp(IRValue t, IRCondition cond);
        public abstract bool? IsEqual(IRValue t);
        public abstract bool? IsGreaterThan(IRValue t);
        public abstract bool? IsLessThan(IRValue t);
        public abstract IRValue BinOp(IRValue t, IROperation operation);
        public abstract IRValue Add(IRValue t);
        public abstract IRValue Sub(IRValue t);
        public abstract IRValue Mul(IRValue t);
        public abstract IRValue Div(IRValue t);
        public abstract IRValue Mod(IRValue t);


        private List<TokenData> _valueTokens { get; }
    }
}
