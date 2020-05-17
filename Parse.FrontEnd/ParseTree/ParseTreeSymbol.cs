using Parse.FrontEnd.Ast;
using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.ParseTree
{
    public abstract class ParseTreeSymbol : IShowable
    {
        public ParseTreeSymbol Parent { get; internal set; } = null;

        public abstract bool IsVirtual { get; }
        public abstract bool HasVirtualChild { get; }
        public abstract AstSymbol ToAst { get; }

        public abstract string ToGrammarString();
        public abstract string ToTreeString(ushort depth = 1);
    }
}
