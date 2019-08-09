using Parse.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.Ast
{
    public abstract class AstSymbol : IShowable
    {
        public abstract string ToGrammarString();
        public abstract string ToTreeString(ushort depth = 1);
    }
}
