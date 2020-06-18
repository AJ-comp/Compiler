namespace Parse.FrontEnd.InterLanguages.LLVM.Models
{
    public class GlobalSSVar : SSVarData
    {
        public override int Offset { get; } = 0;
        public override DataType Type { get; }
        public override object LinkedObject { get; internal set; }

        public GlobalSSVar(DataType type, object linkedObject)
        {
            Type = type;
            LinkedObject = linkedObject;
        }
    }
}
