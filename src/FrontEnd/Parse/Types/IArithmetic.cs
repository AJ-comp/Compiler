using Janglim.Types.Operations;

namespace Janglim.Types
{
    public interface IArithmetic : IDataTypeSpec, IArithmeticOperation
    {
    }

    public interface IArithmetic<T> : IArithmetic
    {
    }
}
