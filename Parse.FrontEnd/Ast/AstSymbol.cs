using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.Ast
{
    public abstract class AstSymbol : IShowable
    {
        public AstSymbol Parent { get; internal set; } = null;

        public abstract string ToGrammarString();
        public abstract string ToTreeString(ushort depth = 1);
    }
}
