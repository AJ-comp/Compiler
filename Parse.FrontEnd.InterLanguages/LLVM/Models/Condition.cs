namespace Parse.FrontEnd.InterLanguages.LLVM.Models
{
    public class Condition : NamelessItem
    {
        public bool ValueRealType => (bool)Value;

        public Condition(bool value) : base(DataType.i1, value)
        {
        }
    }
}
