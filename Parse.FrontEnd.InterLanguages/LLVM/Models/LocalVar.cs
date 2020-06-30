namespace Parse.MiddleEnd.IR.LLVM.Models
{
    public class LocalVar : SSItem, ISSVar
    {
        public int Offset { get; }
        public string Name => "%" + Offset;

        public LocalVar(DataType type, int offset) : base(type)
        {
            Offset = offset;
        }

        public override bool Equals(object obj)
        {
            return obj is LocalVar var &&
                   Offset == var.Offset;
        }

        public override int GetHashCode()
        {
            return -149965190 + Offset.GetHashCode();
        }
    }
}
