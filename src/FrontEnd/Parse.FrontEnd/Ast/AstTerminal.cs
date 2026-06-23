using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.Ast
{
    public class AstTerminal : AstSymbol
    {
        public TokenData Token { get; }

        public override IReadOnlyList<TokenData> AllTokens => new List<TokenData>() { Token };

        public AstTerminal(TokenData tokenData)
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
