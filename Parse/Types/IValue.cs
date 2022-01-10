using AJ.Common.Helpers;
using Parse.Extensions;

namespace Parse.Types
{
    public enum State { Fixed, Dynamic, NotInit, Unknown, Error };

    public interface IValue
    {
        StdType TypeKind { get; }
        object Value { get; }
        State ValueState { get; }
        bool IsInitialized => (ValueState != State.NotInit);


        public string DebuggerDisplay(IValue value)
        {
            return $"{Value} [{TypeKind.ToDescription()}, {ValueState.ToDescription()}]";
        }

        public string GetDebuggerDisplay()
        {
            return $"{TypeKind.ToDescription()}, {ValueState.ToDescription()}";
        }
    }
}
