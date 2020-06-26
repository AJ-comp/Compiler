namespace Parse.FrontEnd.InterLanguages.Datas
{
    public interface IRData
    {
        DataType Type { get; }
        bool IsSigned { get; }
        bool IsNan { get; }
    }
}
