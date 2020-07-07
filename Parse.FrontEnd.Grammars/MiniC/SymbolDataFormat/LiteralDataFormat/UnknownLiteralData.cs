using Parse.MiddleEnd.IR;
using Parse.MiddleEnd.IR.Datas.Types;
using Parse.MiddleEnd.IR.Datas.ValueDatas;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.LiteralDataFormat
{
    public enum UnknownState { NotInitialized = 1, DynamicAllocation = 2 }
    public class UnknownLiteralData : LiteralData
    {
        public UnknownState State { get; }

        public bool IsOnlyNotInit => (State == UnknownState.NotInitialized) ? true : false;
        public bool IsOnlyDynamicAlloc => (State == UnknownState.DynamicAllocation) ? true : false;
        public bool IsIncludeNotInit => ((State & UnknownState.NotInitialized) == UnknownState.NotInitialized) ? true : false;
        public bool IsNotInitAndDynamicAlloc
        {
            get
            {
                if ((State & UnknownState.NotInitialized) != UnknownState.NotInitialized) return false;
                if ((State & UnknownState.DynamicAllocation) != UnknownState.DynamicAllocation) return false;

                return true;
            }
        }

        public override bool IsZero => false;
        public override DType TypeName => DType.Unknown;
        public override object Value => null;
        public override bool Signed => false;
        public override bool IsNan => true;

        public UnknownLiteralData(UnknownState state, IReadOnlyList<TokenData> valueToken) : base(valueToken)
        {
            State = state;
        }

        // NotInit op x(everything) => NotInit
        // DynamicAlloc op DecidedLiteral => DynamicAlloc
        private LiteralData CommonLogic(IRValue right)
        {
            LiteralData result = this;

            if (right is UnknownLiteralData)
            {
                var cRight = right as UnknownLiteralData;

                if (cRight.IsIncludeNotInit) result = right as LiteralData;
            }

            return result;
        }

        public override IRValue Add(IRValue right) => CommonLogic(right);

        public override IRValue Sub(IRValue right) => CommonLogic(right);

        public override IRValue Mul(IRValue right) => CommonLogic(right);

        public override IRValue Div(IRValue right) => CommonLogic(right);

        public override IRValue Mod(IRValue right) => CommonLogic(right);

        public override object Clone() => new UnknownLiteralData(State, ValueToken);

        public override IRValue<Bit> LogicalOp(IRValue t, IRCondition cond)
        {
            throw new NotImplementedException();
        }

        public override bool? IsEqual(IRValue t)
        {
            throw new NotImplementedException();
        }

        public override bool? IsGreaterThan(IRValue t)
        {
            throw new NotImplementedException();
        }

        public override bool? IsLessThan(IRValue t)
        {
            throw new NotImplementedException();
        }

        public override IRValue BinOp(IRValue t, IROperation operation)
        {
            throw new NotImplementedException();
        }
    }
}
