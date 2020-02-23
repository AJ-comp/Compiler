using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;
using static Parse.FrontEnd.Parsers.Datas.LRParsingRowDataFormat;

namespace Parse.FrontEnd.Parsers.Datas
{
    public class LRParsingRowDataFormat : ParsingRowDataFormat<CanonicalItemSet, Symbol, Tuple<ActionDir, object>>
    {
        public enum ActionDir { shift, reduce, epsilon_reduce, moveto, accept, failed }

        public override TerminalSet PossibleTerminalSet
        {
            get
            {
                TerminalSet result = new TerminalSet();

                foreach(var symbol in this.MatchedValueSet.Keys)
                {
                    if (symbol is Terminal) result.Add(symbol as Terminal);
                }

                return result;
            }
        }
        public override HashSet<NonTerminal> PossibleNonTerminalSet
        {
            get
            {
                HashSet<NonTerminal> result = new HashSet<NonTerminal>();

                foreach(var symbol in this.MatchedValueSet.Keys)
                {
                    if (symbol is NonTerminal) result.Add(symbol as NonTerminal);
                }

                return result;
            }
        }

        public LRParsingRowDataFormat(CanonicalItemSet ix, Dictionary<Symbol, Tuple<ActionDir, object>> matchedValueSet) : base(ix, matchedValueSet)
        {
        }
    }
}
