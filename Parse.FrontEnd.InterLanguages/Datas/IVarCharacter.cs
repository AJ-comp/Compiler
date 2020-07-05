using Parse.MiddleEnd.IR.Datas.Types;

namespace Parse.MiddleEnd.IR.Datas
{
    public interface IVarCharacter
    {
        DataType Type { get; }
        string Name { get; }
//        int Block { get; }
//        int Offset { get; }

        int Length { get; }
    }
}
