using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.RegularGrammar;
using System.Collections.Generic;
using System.ComponentModel;

namespace Janglim.FrontEnd.Parsers.Datas.LR
{
    public class LRParsingRowDataFormat : ParsingRowDataFormat<CanonicalState, Symbol, ActionDataList>
    {
        public enum ActionDir 
        {
            [Description("shift")] Shift,
            [Description("reduce")] Reduce,
            [Description("epsilon reduce")] EpsilonReduce, 
            [Description ("goto")] Goto,
            [Description("accept")] Accept,
            [Description("not processed yet")] NotProcessed,
            [Description("failed")] Failed 
        }

        public override TerminalSet PossibleTerminalSet
        {
            get
            {
                TerminalSet result = new TerminalSet();

                foreach(var valueSet in this.MatchedValueSet)
                {
                    if (valueSet.Value[0].Direction == ActionDir.Failed) continue;

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
                    if (valueSet.Value[0].Direction == ActionDir.Failed) continue;

                    if (valueSet.Key is NonTerminal) result.Add(valueSet.Key as NonTerminal);
                }

                return result;
            }
        }

        public LRParsingRowDataFormat(CanonicalState ix, ActionDicSymbolMatched matchedValueSet) : base(ix, matchedValueSet)
        {
        }
    }
}
