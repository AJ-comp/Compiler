using System;

namespace Parse.FrontEnd.Ast
{
    public class ParseTreeTerminal : ParseTreeSymbol
    {
        public TokenData Token { get; }

        public override bool IsVirtual => Token.IsVirtual;
        public override bool HasVirtualChild => false;
        public override AstSymbol ToAst => (Token.Kind.Meaning) ? new AstTerminal(Token) : null;

        public ParseTreeTerminal(TokenData tokenData)
        {
            this.Token = tokenData;
        }

        public override string ToGrammarString()
        {
            throw new NotImplementedException();
        }

        public override string ToTreeString(ushort depth = 1)
        {
            throw new NotImplementedException();
        }

        public override string ToString() => this.Token.Input;
    }
}
