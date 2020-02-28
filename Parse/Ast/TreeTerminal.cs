using System;

namespace Parse.FrontEnd.Ast
{
    public class TreeTerminal : TreeSymbol
    {
        public TokenData Token { get; }

        public TreeTerminal(TokenData tokenData)
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
