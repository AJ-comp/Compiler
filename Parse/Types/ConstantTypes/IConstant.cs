namespace Parse.Types.ConstantTypes
{
    public interface IConstant : IValue
    {
        Constant Casting(DType to);
    }
}
