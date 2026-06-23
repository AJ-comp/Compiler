using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers.Datas.LL
{
    public class LLParsingRowDataFormat : ParsingRowDataFormat<NonTerminal, Terminal, NonTerminalSingle>
    {
        public override TerminalSet PossibleTerminalSet => throw new NotImplementedException();

        public override HashSet<NonTerminal> PossibleNonTerminalSet => throw new NotImplementedException();

        public LLParsingRowDataFormat(NonTerminal row, Dictionary<Terminal, NonTerminalSingle> matchedValueSet) : base(row, matchedValueSet)
        {
        }
    }
}
