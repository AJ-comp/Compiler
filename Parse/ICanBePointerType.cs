namespace Parse
{
    public interface ICanBePointerType
    {
        uint PointerLevel { get; }
        bool IsPointerType => PointerLevel > 0;
    }
}
