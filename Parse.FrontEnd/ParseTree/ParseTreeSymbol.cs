using Parse.FrontEnd.Ast;
using Parse.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Parse.FrontEnd.ParseTree
{
    public abstract class ParseTreeSymbol : IShowable
    {
        public ParseTreeSymbol Parent { get; internal set; } = null;

        public abstract IReadOnlyList<TokenData> AllTokens { get; }

        public abstract bool IsVirtual { get; }
        public abstract bool HasVirtualChild { get; }
        public abstract AstSymbol ToAst { get; }
        public abstract string AllInputDatas { get; }

        public abstract string ToGrammarString();
        public abstract string ToTreeString(ushort depth = 1);
    }
}
