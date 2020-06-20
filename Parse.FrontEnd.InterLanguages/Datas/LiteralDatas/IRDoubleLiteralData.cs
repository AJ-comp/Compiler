namespace Parse.FrontEnd.InterLanguages.Datas
{
    public class IRDoubleLiteralData : IRLiteralData
    {
        public override object Value { get; }
        public override DataType Type => DataType.Double;

        public double ValueRealType => (double)Value;

        public IRDoubleLiteralData(double value)
        {
            Value = value;
        }

        public override IRLiteralData Add(IRLiteralData t)
        {
            return (t is IRDoubleLiteralData) ? new IRDoubleLiteralData(ValueRealType - (t as IRDoubleLiteralData).ValueRealType) :
                                                            new IRDoubleLiteralData(ValueRealType - (t as IRIntegerLiteralData).ValueRealType);
        }

        public override IRLiteralData Div(IRLiteralData t)
        {
            return (t is IRDoubleLiteralData) ? new IRDoubleLiteralData(ValueRealType / (t as IRDoubleLiteralData).ValueRealType) :
                                                            new IRDoubleLiteralData(ValueRealType / (t as IRIntegerLiteralData).ValueRealType);
        }

        public override IRLiteralData Mod(IRLiteralData t)
        {
            return (t is IRDoubleLiteralData) ? new IRDoubleLiteralData(ValueRealType % (t as IRDoubleLiteralData).ValueRealType) :
                                                            new IRDoubleLiteralData(ValueRealType % (t as IRIntegerLiteralData).ValueRealType);
        }

        public override IRLiteralData Mul(IRLiteralData t)
        {
            return (t is IRDoubleLiteralData) ? new IRDoubleLiteralData(ValueRealType * (t as IRDoubleLiteralData).ValueRealType) :
                                                            new IRDoubleLiteralData(ValueRealType * (t as IRIntegerLiteralData).ValueRealType);
        }

        public override IRLiteralData Sub(IRLiteralData t)
        {
            return (t is IRDoubleLiteralData) ? new IRDoubleLiteralData(ValueRealType - (t as IRDoubleLiteralData).ValueRealType) :
                                                            new IRDoubleLiteralData(ValueRealType - (t as IRIntegerLiteralData).ValueRealType);
        }
    }
}
