namespace Parse.Types.ConstantTypes
{
    public abstract class Constant : IConstant
    {
        public object Value { get; set; }
        public State ValueState { get; protected set; } = State.NotInit;
        public bool IsCalculateWithLogicType { get; set; }
        public uint PointerLevel { get; protected set; }
        public abstract DType TypeName { get; }


        protected Constant(object value, State valueState, uint pointerLevel)
        {
            Value = value;
            ValueState = valueState;
            PointerLevel = pointerLevel;
        }
    }
}
