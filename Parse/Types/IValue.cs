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


    public interface IPointerValue
    {
        int PointerLevel { get; }
        int Value { get; }
        State ValueState { get; }
        bool IsInitialized => (ValueState != State.NotInit);
    }
}
