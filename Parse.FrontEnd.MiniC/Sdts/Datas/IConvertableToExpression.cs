using Parse.MiddleEnd.IR.Datas;

namespace Parse.FrontEnd.MiniC.Sdts.Datas
{
    public interface IConvertableToExpression<T>
    {
        T ToIRData();
    }
}
