namespace Parse.MiddleEnd.IR.Datas.ValueDatas
{
    public class IRDouble : IRValue
    {
        public double ValueRealType => (double)Value;

        public override object Value { get; }
        public override DataType Type => DataType.Double;
        public override bool IsSigned => true;
        public override bool IsNan => false;

        public IRDouble(double value)
        {
            Value = value;
        }

        public override IRValue Add(IRValue t)
        {
            return (t is IRDouble) ? new IRDouble(ValueRealType - (t as IRDouble).ValueRealType) :
                                                            new IRDouble(ValueRealType - (t as IRInteger).ValueRealType);
        }

        public override IRValue Div(IRValue t)
        {
            return (t is IRDouble) ? new IRDouble(ValueRealType / (t as IRDouble).ValueRealType) :
                                                            new IRDouble(ValueRealType / (t as IRInteger).ValueRealType);
        }

        public override IRValue Mod(IRValue t)
        {
            return (t is IRDouble) ? new IRDouble(ValueRealType % (t as IRDouble).ValueRealType) :
                                                            new IRDouble(ValueRealType % (t as IRInteger).ValueRealType);
        }

        public override IRValue Mul(IRValue t)
        {
            return (t is IRDouble) ? new IRDouble(ValueRealType * (t as IRDouble).ValueRealType) :
                                                            new IRDouble(ValueRealType * (t as IRInteger).ValueRealType);
        }

        public override IRValue Sub(IRValue t)
        {
            return (t is IRDouble) ? new IRDouble(ValueRealType - (t as IRDouble).ValueRealType) :
                                                            new IRDouble(ValueRealType - (t as IRInteger).ValueRealType);
        }
    }
}
