namespace Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.LiteralDataFormat
{
    public abstract class DecidedLiteralData : LiteralData
    {
        protected enum OpKind { Add, Sub, Mul, Div, Mod };

        protected DecidedLiteralData(TokenData valueToken) : base(valueToken)
        {
        }
    }
}
