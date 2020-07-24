using Parse.MiddleEnd.IR.Datas.Types;

namespace Parse.MiddleEnd.IR.Datas
{
    public interface IRVar : IRData
    {
        string Name { get; }
        //        int Block { get; }
        //        int Offset { get; }
        int Block { get; }
        int Offset { get; }

        int Length { get; }

        ValueData Value { get; }
    }


    public interface IRVar<out T> : IRVar, IRData<T> where T : DataType
    {
    }
}
