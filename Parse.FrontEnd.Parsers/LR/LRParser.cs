using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.Parsers.EventArgs;
using System;

namespace Parse.FrontEnd.Parsers.LR
{
    public abstract class LRParser : Parser
    {
        public abstract CanonicalTable C0 { get; }

        public event EventHandler<ParsingFailedEventArgs> ParsingFailed;

        public LRParser(Grammar grammar) : base(grammar)
        {
        }

        public void OnParsingFailed(ParsingFailedEventArgs e)
        {
            this.ParsingFailed?.Invoke(this, e);
        }
    }
}
