namespace Parse.FrontEnd.InterLanguages.LLVM.Models
{
    public abstract class LocalSSVar : SSVarData
    {
        public override int Offset { get; }
        public override object LinkedObject { get; internal set; }

        public LocalSSVar(int offset)
        {
            Offset = offset;
        }

        public LocalSSVar(int offset, object linkedObject) : this(offset)
        {
            LinkedObject = linkedObject;
        }
    }
}
