using Parse.Types;
using Parse.Types.VarTypes;

namespace Parse.MiddleEnd.IR.Datas
{
    public interface IRVar : IVariable
    {
        string Name { get; }
        int Block { get; }
        int Offset { get; }
        int Length { get; }
    }


    public interface IRIntegerVar : IRVar, IByte, IShort, IInt
    {
    }

    public interface IRDoubleVar : IRVar, IDouble
    {
    }
}
