using Parse.Types.ConstantTypes;

namespace Parse.Types
{
    public interface IString : IDataTypeSpec
    {
        IConstant Add(IConstant operand);
    }
}
