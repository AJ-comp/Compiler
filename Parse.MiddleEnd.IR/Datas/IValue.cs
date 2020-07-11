namespace Parse.MiddleEnd.IR.Datas
{
    public interface IValue
    {
        object Value { get; }
        bool Signed { get; }
        bool IsNan { get; }

        // Value == null => NotInit
        // IsNan == true => Nan value
        public bool IsUnKnown => (Value == null) || (IsNan == true);
    }
}
