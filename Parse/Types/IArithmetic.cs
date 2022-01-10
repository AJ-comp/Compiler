using Parse.Types.Operations;

namespace Parse.Types
{
    public interface IArithmetic : IDataTypeSpec, IArithmeticOperation
    {
    }

    public interface IArithmetic<T> : IArithmetic
    {
    }
}
