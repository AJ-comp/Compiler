namespace Parse.MiddleEnd.IR.LLVM.Models
{
    public class RealNumber : NamelessItem
    {
        public double ValueRealType => (double)Value;

        public RealNumber(double value) : base(DataType.Double, value)
        {
        }
    }
}
