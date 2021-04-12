namespace Parse.Types.ConstantTypes
{
    public interface IConstant : IValue
    {
        Constant Casting(StdType to);
    }
}
