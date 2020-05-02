using System;

namespace Parse.FrontEnd.Ast
{
    public class TreeTerminal : TreeSymbol
    {
        private bool _isVirtual = false;

        public TokenData Token { get; }

        public override bool IsVirtual => _isVirtual;
        public override bool HasVirtualChild => false;

        public TreeTerminal(TokenData tokenData, bool isVirtual = false)
        {
            this.Token = tokenData;
            _isVirtual = isVirtual;
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
