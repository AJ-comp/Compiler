namespace Parse.MiddleEnd.IR.LLVM.Models
{
    public class Integer : NamelessItem
    {
        public int ValueRealType => (int)Value;

        public Integer(int value) : base(DataType.i32, value)
        {
        }
    }
}
