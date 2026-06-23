namespace Parse.MiddleEnd.IR.Datas
{
    public interface IUseDefChainable
    {
        string Name { get; }
        uint PointerLevel { get; set; }
    }
}
