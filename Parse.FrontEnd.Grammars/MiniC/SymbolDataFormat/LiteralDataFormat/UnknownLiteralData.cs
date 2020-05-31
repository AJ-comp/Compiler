using System;

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

        public UnknownLiteralData(UnknownState state, TokenData valueToken) : base(valueToken)
        {
            State = state;
        }

        // NotInit op x(everything) => NotInit
        // DynamicAlloc op DecidedLiteral => DynamicAlloc
        private LiteralData CommonLogic(LiteralData right)
        {
            LiteralData result = this;

            if (right is UnknownLiteralData)
            {
                var cRight = right as UnknownLiteralData;

                if (cRight.IsIncludeNotInit) result = right;
            }

            return result;
        }

        public override LiteralData Add(LiteralData right) => CommonLogic(right);

        public override LiteralData Sub(LiteralData right) => CommonLogic(right);

        public override LiteralData Mul(LiteralData right) => CommonLogic(right);

        public override LiteralData Div(LiteralData right) => CommonLogic(right);

        public override LiteralData Mod(LiteralData right) => CommonLogic(right);

        public override object Clone() => new UnknownLiteralData(State, ValueToken);
    }
}
