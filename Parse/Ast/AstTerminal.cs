using Parse.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.Ast
{
    public class AstTerminal : AstSymbol
    {
        public TokenData Token { get; }

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
