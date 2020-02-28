using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.Ast
{
    public abstract class TreeSymbol : IShowable
    {
        public abstract string ToGrammarString();
        public abstract string ToTreeString(ushort depth = 1);
    }
}
