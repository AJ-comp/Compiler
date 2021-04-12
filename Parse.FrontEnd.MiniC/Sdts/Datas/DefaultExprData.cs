using Parse.Types;
using Parse.Types.ConstantTypes;

namespace Parse.FrontEnd.MiniC.Sdts.Datas
{
    public class DefaultExprData : IExprExpression
    {
        public IConstant Result { get; }

        public DefaultExprData(StdType type)
        {
            Result = new NotInitConstant(type);
        }
    }
}
