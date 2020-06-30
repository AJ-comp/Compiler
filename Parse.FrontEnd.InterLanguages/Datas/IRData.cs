namespace Parse.MiddleEnd.IR.Datas
{
    public interface IRData
    {
        DataType Type { get; }
        bool IsSigned { get; }
        bool IsNan { get; }
    }
}
