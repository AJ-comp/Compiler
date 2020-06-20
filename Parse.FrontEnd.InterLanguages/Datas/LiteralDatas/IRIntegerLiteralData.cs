namespace Parse.FrontEnd.InterLanguages.Datas
{
    public class IRIntegerLiteralData : IRLiteralData
    {
        private DataType _type = DataType.i32;

        public override object Value { get; }
        public override DataType Type => _type;

        public int ValueRealType => (int)Value;

        public IRIntegerLiteralData(int value)
        {
            Value = value;
        }

        public IRIntegerLiteralData(DataType type, int value) : this(value)
        {
            _type = type;
        }

        public override IRLiteralData Add(IRLiteralData t)
        {
            IRLiteralData result = null;
            if (t is IRDoubleLiteralData) result = new IRDoubleLiteralData(ValueRealType + (t as IRDoubleLiteralData).ValueRealType);
            else result = new IRIntegerLiteralData(ValueRealType + (t as IRIntegerLiteralData).ValueRealType);

            return result;
        }

        public override IRLiteralData Div(IRLiteralData t)
        {
            IRLiteralData result = null;
            if (t is IRDoubleLiteralData) result = new IRDoubleLiteralData(ValueRealType / (t as IRDoubleLiteralData).ValueRealType);
            else result = new IRIntegerLiteralData(ValueRealType / (t as IRIntegerLiteralData).ValueRealType);

            return result;
        }

        public override IRLiteralData Mod(IRLiteralData t)
        {
            IRLiteralData result = null;
            if (t is IRDoubleLiteralData) result = new IRDoubleLiteralData(ValueRealType % (t as IRDoubleLiteralData).ValueRealType);
            else result = new IRIntegerLiteralData(ValueRealType % (t as IRIntegerLiteralData).ValueRealType);

            return result;
        }

        public override IRLiteralData Mul(IRLiteralData t)
        {
            IRLiteralData result = null;
            if (t is IRDoubleLiteralData) result = new IRDoubleLiteralData(ValueRealType * (t as IRDoubleLiteralData).ValueRealType);
            else result = new IRIntegerLiteralData(ValueRealType * (t as IRIntegerLiteralData).ValueRealType);

            return result;
        }

        public override IRLiteralData Sub(IRLiteralData t)
        {
            IRLiteralData result = null;
            if (t is IRDoubleLiteralData) result = new IRDoubleLiteralData(ValueRealType - (t as IRDoubleLiteralData).ValueRealType);
            else result = new IRIntegerLiteralData(ValueRealType - (t as IRIntegerLiteralData).ValueRealType);

            return result;
        }
    }
}
