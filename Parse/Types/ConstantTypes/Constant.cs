namespace Parse.Types.ConstantTypes
{
    public abstract class Constant : IConstant
    {
        public object Value { get; set; }
        public State ValueState { get; protected set; } = State.NotInit;
        public abstract DType TypeName { get; }
        public abstract bool AlwaysTrue { get; }
        public abstract bool AlwaysFalse { get; }


        protected Constant(object value, State valueState)
        {
            Value = value;
            ValueState = valueState;
        }

        public abstract Constant Casting(DType to);
    }
}
