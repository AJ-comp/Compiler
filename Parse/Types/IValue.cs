namespace Parse.Types
{
    public enum State { Fixed, Dynamic, NotInit, Unknown };

    public interface IValue
    {
        DType TypeName { get; }
        object Value { get; }
        State ValueState { get; }
        bool IsInitialized => (ValueState != State.NotInit);
    }
}
