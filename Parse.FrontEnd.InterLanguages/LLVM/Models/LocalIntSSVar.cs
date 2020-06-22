namespace Parse.FrontEnd.InterLanguages.LLVM.Models
{
    public class LocalIntSSVar : LocalSSVar
    {
        public bool IsUnsigned { get; } = false;
        public override DataType Type => DataType.i32;

        public LocalIntSSVar(int offset) : base(offset)
        {
        }

        public LocalIntSSVar(int offset, object linkedObject) : base(offset, linkedObject)
        {
        }

        public LocalIntSSVar(int offset, bool isUnsigned, object linkedObject) : this(offset, linkedObject)
        {
            IsUnsigned = isUnsigned;
        }
    }
}
