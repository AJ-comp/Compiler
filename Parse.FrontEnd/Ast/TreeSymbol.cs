using Parse.FrontEnd.RegularGrammar;

namespace Parse.FrontEnd.Ast
{
    public abstract class TreeSymbol : IShowable
    {
        public TreeSymbol Parent { get; internal set; } = null;

        public abstract bool IsVirtual { get; }
        public abstract bool HasVirtualChild { get; }

        public abstract string ToGrammarString();
        public abstract string ToTreeString(ushort depth = 1);
    }
}
