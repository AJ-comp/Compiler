using Parse.FrontEnd.Parsers.Collections;
using Parse.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;
using static Parse.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Parse.FrontEnd.Parsers.Datas.LR
{
    public class LRParsingRowDataFormat : ParsingRowDataFormat<CanonicalItemSet, Symbol, Tuple<ActionDir, object>>
    {
        public enum ActionDir { shift, reduce, epsilon_reduce, moveto, accept, failed }

        public override TerminalSet PossibleTerminalSet
        {
            get
            {
                TerminalSet result = new TerminalSet();

                foreach(var valueSet in this.MatchedValueSet)
                {
                    if (valueSet.Value.Item1 == ActionDir.failed) continue;

                    if (valueSet.Key is Terminal) result.Add(valueSet.Key as Terminal);
                }

                return result;
            }
        }
        public override HashSet<NonTerminal> PossibleNonTerminalSet
        {
            get
            {
                HashSet<NonTerminal> result = new HashSet<NonTerminal>();

                foreach(var valueSet in this.MatchedValueSet)
                {
                    if (valueSet.Value.Item1 == ActionDir.failed) continue;

                    if (valueSet.Key is NonTerminal) result.Add(valueSet.Key as NonTerminal);
                }

                return result;
            }
        }

        public LRParsingRowDataFormat(CanonicalItemSet ix, Dictionary<Symbol, Tuple<ActionDir, object>> matchedValueSet) : base(ix, matchedValueSet)
        {
        }
    }
}
