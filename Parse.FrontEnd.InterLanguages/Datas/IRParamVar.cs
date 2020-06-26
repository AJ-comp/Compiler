namespace Parse.FrontEnd.InterLanguages.Datas
{
    public enum VarPassWay { CallByValue, CallByAddress };

    public class IRParamVar : IRVar
    {
        public VarPassWay PassWay { get; }
        public string Comment { get; }

        public override bool IsSigned => false;
        public override bool IsNan => false;

        public IRParamVar(int bIndex, int oIndex, DataType type, string name, int length, VarPassWay passWay, string comment)
            : base(type, name, bIndex, oIndex, length)
        {
            PassWay = passWay;
            Comment = comment;
        }

    }
}
