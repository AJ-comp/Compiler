using Janglim.Types.Operations;

namespace Janglim.Types
{
    public interface IBit : IDataTypeSpec, ILogicalOperation, IBitwiseOperation
    {
        int IDataTypeSpec.Size => 1;
    }

    public interface IBit<T> : IBit
    {
    }
}
