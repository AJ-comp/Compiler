namespace Parse.MiddleEnd.IR.Datas.ValueDatas
{
    public class IRCond : IRValue
    {
        public override bool IsSigned => false;
        public override bool IsNan => false;

        public override object Value => throw new System.NotImplementedException();

        public override DataType Type => throw new System.NotImplementedException();

        public IRCond(bool value = false) : base(DataType.i1, string.Empty, 1, 1, 1)
        {
            Value = value;
        }

        public override IRValue Add(IRValue t)
        {
            throw new System.NotImplementedException();
        }

        public override IRValue Sub(IRValue t)
        {
            throw new System.NotImplementedException();
        }

        public override IRValue Mul(IRValue t)
        {
            throw new System.NotImplementedException();
        }

        public override IRValue Div(IRValue t)
        {
            throw new System.NotImplementedException();
        }

        public override IRValue Mod(IRValue t)
        {
            throw new System.NotImplementedException();
        }
    }
}
