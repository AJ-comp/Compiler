namespace Parse.FrontEnd.InterLanguages.LLVM.Models
{
    public class LocalVar : SSItem, ISSVar
    {
        public int Offset { get; }
        public string Name => "%" + Offset;

        public LocalVar(DataType type, int offset) : base(type)
        {
            Offset = offset;
        }
    }
}
