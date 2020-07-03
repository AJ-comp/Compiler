using Parse.FrontEnd.InterLanguages.Datas.Types;

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
    }


    public interface IRVar<out T> : IRVar where T : DataType
    {
    }
}
