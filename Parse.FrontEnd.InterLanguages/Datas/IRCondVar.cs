namespace Parse.FrontEnd.InterLanguages.Datas
{
    public class IRCondVar : IRVar
    {
        public bool Value { get; }

        public override bool IsSigned => false;
        public override bool IsNan => false;

        public IRCondVar(bool value = false) : base(DataType.i1, string.Empty, 1, 1, 1)
        {
            Value = value;
        }
    }
}
