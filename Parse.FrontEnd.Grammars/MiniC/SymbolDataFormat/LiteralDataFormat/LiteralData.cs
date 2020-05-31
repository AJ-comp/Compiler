using System;

namespace Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.LiteralDataFormat
{
    public abstract class LiteralData : ICloneable
    {
        public string LiteralName => ValueToken?.Input;

        public TokenData ValueToken { get; }

        public bool IsVirtual { get; protected set; } = false;

        protected LiteralData(TokenData valueToken)
        {
            ValueToken = valueToken;
        }

        public abstract LiteralData Add(LiteralData right);
        public abstract LiteralData Sub(LiteralData right);
        public abstract LiteralData Mul(LiteralData right);
        public abstract LiteralData Div(LiteralData right);
        public abstract LiteralData Mod(LiteralData right);
        public abstract object Clone();
    }
}
