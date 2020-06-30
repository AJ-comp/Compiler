namespace Parse.MiddleEnd.IR
{
    public interface IRUnit
    {
        string Comment { get; }

        string ToFormatString();
    }
}
