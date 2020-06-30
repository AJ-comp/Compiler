namespace Parse.MiddleEnd.IR.Datas.ValueDatas
{
    public class IRInteger : IRValue
    {
        private DataType _type = DataType.i32;
        private bool _isSigned = false;

        public int ValueRealType => (int)Value;

        public override object Value { get; }
        public override DataType Type => _type;
        public override bool IsSigned => _isSigned;
        public override bool IsNan => false;


        public IRInteger(int value)
        {
            Value = value;
        }

        public IRInteger(uint value)
        {
            _isSigned = true;
            Value = value;
        }

        public IRInteger(DataType type, int value) : this(value)
        {
            _type = type;
        }

        public override IRValue Add(IRValue t)
        {
            IRValue result = null;
            if (t is IRDouble) result = new IRDouble(ValueRealType + (t as IRDouble).ValueRealType);
            else result = new IRInteger(ValueRealType + (t as IRInteger).ValueRealType);

            return result;
        }

        public override IRValue Div(IRValue t)
        {
            IRValue result = null;
            if (t is IRDouble) result = new IRDouble(ValueRealType / (t as IRDouble).ValueRealType);
            else result = new IRInteger(ValueRealType / (t as IRInteger).ValueRealType);

            return result;
        }

        public override IRValue Mod(IRValue t)
        {
            IRValue result = null;
            if (t is IRDouble) result = new IRDouble(ValueRealType % (t as IRDouble).ValueRealType);
            else result = new IRInteger(ValueRealType % (t as IRInteger).ValueRealType);

            return result;
        }

        public override IRValue Mul(IRValue t)
        {
            IRValue result = null;
            if (t is IRDouble) result = new IRDouble(ValueRealType * (t as IRDouble).ValueRealType);
            else result = new IRInteger(ValueRealType * (t as IRInteger).ValueRealType);

            return result;
        }

        public override IRValue Sub(IRValue t)
        {
            IRValue result = null;
            if (t is IRDouble) result = new IRDouble(ValueRealType - (t as IRDouble).ValueRealType);
            else result = new IRInteger(ValueRealType - (t as IRInteger).ValueRealType);

            return result;
        }
    }
}
