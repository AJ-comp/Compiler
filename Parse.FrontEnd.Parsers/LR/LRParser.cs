using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers.Collections;

namespace Parse.FrontEnd.Parsers.LR
{
    public abstract class LRParser : Parser
    {
        public abstract CanonicalTable C0 { get; }

        public LRParser(Grammar grammar) : base(grammar)
        {
        }
    }
}
