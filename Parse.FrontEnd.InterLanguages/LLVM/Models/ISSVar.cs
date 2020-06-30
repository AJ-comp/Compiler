namespace Parse.MiddleEnd.IR.LLVM.Models
{
    public interface ISSVar
    {
        DataType Type { get; }
        string Name { get; }
    }
}
