using Janglim.FrontEnd.RegularGrammar;
using System.Collections.Generic;

namespace Janglim.FrontEnd.Parsers.Datas
{
    public abstract class ParsingRowDataFormat<ROW, COL, VAL>
    {
        public ROW Row { get; }
        public Dictionary<COL, VAL> MatchedValueSet { get; }
        public abstract TerminalSet PossibleTerminalSet { get; }
        public abstract HashSet<NonTerminal> PossibleNonTerminalSet { get; }

        protected ParsingRowDataFormat(ROW row, Dictionary<COL, VAL> matchedValueSet)
        {
            Row = row;
            MatchedValueSet = matchedValueSet;
        }
    }
}
