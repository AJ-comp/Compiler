using Parse.Types.Operations;

namespace Parse.Types
{
    public interface IBit : IDataTypeSpec, ILogicalOperation, IBitwiseOperation
    {
        int IDataTypeSpec.Size => 1;
    }

    public interface IBit<T> : IBit
    {
    }
}
