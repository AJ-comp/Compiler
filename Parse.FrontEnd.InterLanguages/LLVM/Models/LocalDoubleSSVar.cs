namespace Parse.FrontEnd.InterLanguages.LLVM.Models
{
    public class LocalDoubleSSVar : LocalSSVar
    {
        public override DataType Type => DataType.Double;

        public LocalDoubleSSVar(int offset) : base(offset)
        {
        }

        public LocalDoubleSSVar(int offset, object linkedObject) : base(offset, linkedObject)
        {
        }
    }
}
