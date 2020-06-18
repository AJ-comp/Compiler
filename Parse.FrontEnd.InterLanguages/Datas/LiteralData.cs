namespace Parse.FrontEnd.InterLanguages.Datas
{
    public class LiteralData
    {
        public DataType Type { get; }
        public object Value { get; }

        public LiteralData(DataType type, object value)
        {
            Type = type;
            Value = value;
        }
    }
}
