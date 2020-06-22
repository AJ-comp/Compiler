namespace Parse.FrontEnd.InterLanguages.LLVM.Models
{
    public class CondVar : LocalSSVar
    {
        public override DataType Type => DataType.i8;

        public CondVar(int offset) : base(offset)
        {
        }

        public CondVar(int offset, object linkedObject) : base(offset, linkedObject)
        {
        }
    }
}
