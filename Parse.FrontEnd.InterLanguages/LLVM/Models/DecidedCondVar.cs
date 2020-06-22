namespace Parse.FrontEnd.InterLanguages.LLVM.Models
{
    public class DecidedCondVar : CondVar
    {
        public DecidedCondVar(bool value) : base(-1)
        {
            Value = value;
        }

        public DecidedCondVar(bool value, object linkedObject) : base(-1, linkedObject)
        {
            Value = value;
        }

        public bool Value { get; }
    }
}
