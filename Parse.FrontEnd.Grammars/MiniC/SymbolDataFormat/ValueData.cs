using Parse.MiddleEnd.IR.Datas.Types;
using Parse.MiddleEnd.IR.Datas.ValueDatas;

namespace Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat
{
    public abstract class ValueData : IRValue
    {
        public bool IsUnknown { get; } = false;
        public bool IsCalculatable => (!IsNan && !IsUnknown);

        public UnknownState UnknownState { get; }

        public bool IsOnlyNotInit => (UnknownState == UnknownState.NotInitialized);
        public bool IsOnlyDynamicAlloc => (UnknownState == UnknownState.DynamicAllocation);
        public bool IsIncludeNotInit => ((UnknownState & UnknownState.NotInitialized) == UnknownState.NotInitialized);
        public bool IsNotInitAndDynamicAlloc
        {
            get
            {
                if ((UnknownState & UnknownState.NotInitialized) != UnknownState.NotInitialized) return false;
                if ((UnknownState & UnknownState.DynamicAllocation) != UnknownState.DynamicAllocation) return false;

                return true;
            }
        }

        public abstract bool IsZero { get; }
        public abstract DType TypeName { get; }
        public abstract object Value { get; }
        public abstract bool Signed { get; }
        public abstract bool IsNan { get; }

        public abstract IRValue Add(IRValue t);
        public abstract IRValue Div(IRValue t);
        public abstract IRValue Mod(IRValue t);
        public abstract IRValue Mul(IRValue t);
        public abstract IRValue Sub(IRValue t);
    }
}
