using Parse.Types.Operations;

namespace Parse.Types
{
    public interface IIntegerKind : IArithmetic, IBitwiseOperation
    {
        public bool Signed { get; }
    }
}
