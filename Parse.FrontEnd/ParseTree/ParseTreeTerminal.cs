using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.ParseTree
{
    public class ParseTreeTerminal : ParseTreeSymbol
    {
        public TokenData Token { get; }

        public override bool IsVirtual => Token.IsVirtual;
        public override bool HasVirtualChild => false;
        public override AstSymbol ToAst => (IsMeaning) ? new AstTerminal(Token) : null;
        public override IReadOnlyList<TokenData> AllTokens => new List<TokenData>() { Token };
        public override string AllInputDatas => Token.Input;
        public override bool IsMeaning => (Token.Kind.Meaning);

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
