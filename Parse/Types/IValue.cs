﻿namespace Parse.Types
{
    public enum State { Fixed, Dynamic, NotInit, Unknown };

    public interface IValue
    {
        StdType TypeKind { get; }
        object Value { get; }
        State ValueState { get; }
        bool IsInitialized => (ValueState != State.NotInit);


        public string DebuggerDisplay(IValue value)
        {
            return string.Format("{0} [{1}, {2}]",
                                            Value,
                                            Helper.GetEnumDescription(TypeKind),
                                            Helper.GetEnumDescription(ValueState));
        }
    }
}
