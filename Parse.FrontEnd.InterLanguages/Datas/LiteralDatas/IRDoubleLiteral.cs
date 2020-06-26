namespace Parse.FrontEnd.InterLanguages.Datas
{
    public class IRDoubleLiteral : IRLiteral
    {
        public double ValueRealType => (double)Value;

        public override object Value { get; }
        public override DataType Type => DataType.Double;
        public override bool IsSigned => true;
        public override bool IsNan => false;

        public IRDoubleLiteral(double value)
        {
            Value = value;
        }

        public override IRLiteral Add(IRLiteral t)
        {
            return (t is IRDoubleLiteral) ? new IRDoubleLiteral(ValueRealType - (t as IRDoubleLiteral).ValueRealType) :
                                                            new IRDoubleLiteral(ValueRealType - (t as IRIntegerLiteral).ValueRealType);
        }

        public override IRLiteral Div(IRLiteral t)
        {
            return (t is IRDoubleLiteral) ? new IRDoubleLiteral(ValueRealType / (t as IRDoubleLiteral).ValueRealType) :
                                                            new IRDoubleLiteral(ValueRealType / (t as IRIntegerLiteral).ValueRealType);
        }

        public override IRLiteral Mod(IRLiteral t)
        {
            return (t is IRDoubleLiteral) ? new IRDoubleLiteral(ValueRealType % (t as IRDoubleLiteral).ValueRealType) :
                                                            new IRDoubleLiteral(ValueRealType % (t as IRIntegerLiteral).ValueRealType);
        }

        public override IRLiteral Mul(IRLiteral t)
        {
            return (t is IRDoubleLiteral) ? new IRDoubleLiteral(ValueRealType * (t as IRDoubleLiteral).ValueRealType) :
                                                            new IRDoubleLiteral(ValueRealType * (t as IRIntegerLiteral).ValueRealType);
        }

        public override IRLiteral Sub(IRLiteral t)
        {
            return (t is IRDoubleLiteral) ? new IRDoubleLiteral(ValueRealType - (t as IRDoubleLiteral).ValueRealType) :
                                                            new IRDoubleLiteral(ValueRealType - (t as IRIntegerLiteral).ValueRealType);
        }
    }
}
