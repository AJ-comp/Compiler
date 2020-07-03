namespace Parse.MiddleEnd.IR.Datas
{
    public interface IValue
    {
        object Value { get; }
        bool Signed { get; }
        bool IsNan { get; }
    }
}
