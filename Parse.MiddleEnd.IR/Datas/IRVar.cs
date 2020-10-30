using Parse.Types;

namespace Parse.MiddleEnd.IR.Datas
{
    public interface IRVar : IValue
    {
        string Name { get; }
        int Block { get; set; }
        int Offset { get; set; }
        int Length { get; }
        uint PointerLevel { get; set; }
    }

    public interface IRSignableVar : IRVar
    {
        public bool Signed { get; }
    }

    public interface IRDoubleVar : IRVar
    {
        public bool Nan { get; }
    }
}
