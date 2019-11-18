using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.Parsers.Datas
{
    public class LRParsingData
    {
        public enum ActionDir { shift, reduce, epsilon_reduce, moveto, accept, failed }

        public Canonical Ix { get; }
        public Dictionary<Symbol, Tuple<ActionDir, object>> MatchedValueSet { get; }
        public TerminalSet PossibleTerminalSet
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
        public HashSet<NonTerminal> PossibleNonTerminalSet
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

        public LRParsingData(Canonical ix, Dictionary<Symbol, Tuple<ActionDir, object>> matchedValueSet)
        {
            this.Ix = ix;
            this.MatchedValueSet = matchedValueSet;
        }

        /*
        public Tuple<Symbol, ActionInfo, object> GetMatchedValue(Symbol symbol)
        {

        }
        */
    }
}
