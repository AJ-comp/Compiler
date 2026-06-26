using Janglim.Types.Operations;

namespace Janglim.Types
{
    public interface IIntegerKind : IArithmetic, IBitwiseOperation
    {
        public bool Signed { get; }
    }


    public interface IIntegerKind<T> : IIntegerKind
    {
    }
}
