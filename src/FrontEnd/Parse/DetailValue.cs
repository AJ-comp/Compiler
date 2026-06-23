using System.ComponentModel;
using System.Diagnostics;

namespace Parse
{
    public enum UnknownState
    {
        [Description("not initialized")] NotInitialized = 1, 
        [Description("dynamic allocation")] DynamicAllocation = 2
    }

    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public class DetailValue : ICloneable<DetailValue>
    {
        public DetailValue()
        {
            IsUnknown = true;
        }

        public DetailValue(object value, bool bSigned = true)
        {
            Signed = bSigned;
            IsUnknown = false;
            Value = value;
        }

        public bool IsUnknown { get; }
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

        public bool? IsZero
        {
            get
            {
                if (Value == null) return null;
                return ((int)Value == 0);
            }
        }
        public object Value { get; set; }
        public bool Signed { get; }
        public bool IsNan { get; }
        public bool IsCorrect { get; set; } = false;

        public DetailValue Clone()
        {
            return (IsUnknown) ? new DetailValue() : new DetailValue(Value, Signed);
        }

        private string DebuggerDisplay
            => string.Format("Value : {0}, IsSigned : {1}, IsUnknown : {2}", Value, Signed, IsUnknown);
    }
}
