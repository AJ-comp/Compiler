using Parse.MiddleEnd.IR.Datas.Types;

namespace Parse.MiddleEnd.IR.Datas
{
    public interface IRData
    {
        DType TypeName { get; }
    }


    public interface IRData<out T> where T : DataType
    {
        bool IsNan { get; }
    }
}
