namespace Parse.FrontEnd.InterLanguages.Datas
{
    public class IRIntegerLiteral : IRLiteral
    {
        private DataType _type = DataType.i32;
        private bool _isSigned = false;

        public int ValueRealType => (int)Value;

        public override object Value { get; }
        public override DataType Type => _type;
        public override bool IsSigned => _isSigned;
        public override bool IsNan => false;


        public IRIntegerLiteral(int value)
        {
            Value = value;
        }

        public IRIntegerLiteral(uint value)
        {
            _isSigned = true;
            Value = value;
        }

        public IRIntegerLiteral(DataType type, int value) : this(value)
        {
            _type = type;
        }

        public override IRLiteral Add(IRLiteral t)
        {
            IRLiteral result = null;
            if (t is IRDoubleLiteral) result = new IRDoubleLiteral(ValueRealType + (t as IRDoubleLiteral).ValueRealType);
            else result = new IRIntegerLiteral(ValueRealType + (t as IRIntegerLiteral).ValueRealType);

            return result;
        }

        public override IRLiteral Div(IRLiteral t)
        {
            IRLiteral result = null;
            if (t is IRDoubleLiteral) result = new IRDoubleLiteral(ValueRealType / (t as IRDoubleLiteral).ValueRealType);
            else result = new IRIntegerLiteral(ValueRealType / (t as IRIntegerLiteral).ValueRealType);

            return result;
        }

        public override IRLiteral Mod(IRLiteral t)
        {
            IRLiteral result = null;
            if (t is IRDoubleLiteral) result = new IRDoubleLiteral(ValueRealType % (t as IRDoubleLiteral).ValueRealType);
            else result = new IRIntegerLiteral(ValueRealType % (t as IRIntegerLiteral).ValueRealType);

            return result;
        }

        public override IRLiteral Mul(IRLiteral t)
        {
            IRLiteral result = null;
            if (t is IRDoubleLiteral) result = new IRDoubleLiteral(ValueRealType * (t as IRDoubleLiteral).ValueRealType);
            else result = new IRIntegerLiteral(ValueRealType * (t as IRIntegerLiteral).ValueRealType);

            return result;
        }

        public override IRLiteral Sub(IRLiteral t)
        {
            IRLiteral result = null;
            if (t is IRDoubleLiteral) result = new IRDoubleLiteral(ValueRealType - (t as IRDoubleLiteral).ValueRealType);
            else result = new IRIntegerLiteral(ValueRealType - (t as IRIntegerLiteral).ValueRealType);

            return result;
        }
    }
}
