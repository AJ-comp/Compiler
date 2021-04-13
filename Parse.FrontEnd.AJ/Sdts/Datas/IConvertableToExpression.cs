using Parse.MiddleEnd.IR.Datas;

namespace Parse.FrontEnd.AJ.Sdts.Datas
{
    public interface IConvertableToExpression<T>
    {
        T ToIRData();
    }
}
