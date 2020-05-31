namespace Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.LiteralDataFormat
{
    public class IntLiteralData : DecidedLiteralData
    {
        private int _value = 0;

        public int Value => (IsVirtual) ? _value : System.Convert.ToInt32(ValueToken?.Input);

        public IntLiteralData(TokenData token) : base(token)
        {
        }

        public IntLiteralData(int value) : base(null)
        {
            _value = value;
            IsVirtual = true;
        }

        private static LiteralData CoreLogic(IntLiteralData left, LiteralData right, OpKind opKind)
        {
            if (right is IntLiteralData)
            {
                var cRight = right as IntLiteralData;

                int value = (opKind == OpKind.Add) ? (int)left.Value + (int)cRight.Value :
                                (opKind == OpKind.Sub) ? (int)left.Value - (int)cRight.Value :
                                (opKind == OpKind.Mul) ? (int)left.Value * (int)cRight.Value :
                                (opKind == OpKind.Div) ? (int)left.Value / (int)cRight.Value :
                                (int)left.Value % (int)cRight.Value;

                return new IntLiteralData(value);
            }
            else if (right is UnknownLiteralData)
            {
                var cRight = right as UnknownLiteralData;
                return new UnknownLiteralData(cRight.State, null);
            }

            return null;
        }

        public override LiteralData Add(LiteralData right) => CoreLogic(this, right, OpKind.Add);
        public override LiteralData Sub(LiteralData right) => CoreLogic(this, right, OpKind.Sub);
        public override LiteralData Mul(LiteralData right) => CoreLogic(this, right, OpKind.Mul);
        public override LiteralData Div(LiteralData right) => CoreLogic(this, right, OpKind.Div);
        public override LiteralData Mod(LiteralData right) => CoreLogic(this, right, OpKind.Mod);

        public override string ToString() => Value.ToString();

        public override object Clone() => (IsVirtual) ? new IntLiteralData(Value) : new IntLiteralData(ValueToken);
    }
}
